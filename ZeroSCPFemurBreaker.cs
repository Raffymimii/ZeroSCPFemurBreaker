using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using Mirror;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZeroSCPFemurBreaker
{
    public class ZeroSCPFemurBreaker : Plugin<Config>
    {
        public override string Name => "ZeroSCPFemurBreaker";
        public override string Author => "Raffymimi";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(5, 0, 0);

        private EventHandlers eventHandlers;

        public override void OnEnabled()
        {
            eventHandlers = new EventHandlers(Config);
            Exiled.Events.Handlers.Server.RoundStarted += eventHandlers.OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += eventHandlers.OnRoundEnded;
            Exiled.Events.Handlers.Player.InteractingDoor += eventHandlers.OnInteractingDoor;
            Exiled.Events.Handlers.Player.ActivatingGenerator += eventHandlers.OnActivatingGenerator;
            Exiled.Events.Handlers.Player.StoppingGenerator += eventHandlers.OnStoppingGenerator;
            Exiled.Events.Handlers.Player.TogglingNoClip += eventHandlers.OnTogglingNoClip;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= eventHandlers.OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= eventHandlers.OnRoundEnded;
            Exiled.Events.Handlers.Player.InteractingDoor -= eventHandlers.OnInteractingDoor;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= eventHandlers.OnActivatingGenerator;
            Exiled.Events.Handlers.Player.StoppingGenerator -= eventHandlers.OnStoppingGenerator;
            Exiled.Events.Handlers.Player.TogglingNoClip -= eventHandlers.OnTogglingNoClip;

            eventHandlers = null;

            base.OnDisabled();
        }
    }

    public class EventHandlers
    {
        private readonly Config config;

        public EventHandlers(Config pluginConfig)
        {
            config = pluginConfig;
        }

        private bool is106CellLocked = true;
        private bool hasStartedProcess = false;
        private bool actionTriggered = false;
        private readonly Dictionary<Generator, float> generatorActivationTimes = new Dictionary<Generator, float>();
        private readonly Dictionary<Generator, float> generatorCooldowns = new Dictionary<Generator, float>();
        private int activeGeneratorsCount = 0;

        private ReferenceHub femurBreaker;
        private CustomAudioPlayer audioPlayer;

        public void OnRoundStarted()
        {
            Timing.CallDelayed(10f, () =>
            {
                GameObject femurBrakerButton = GameObject.Find(config.GameObjectName);

                if (femurBrakerButton != null)
                {
                    CheckForCollider(femurBrakerButton);
                }
            });

            Lock106CellDoor();
            actionTriggered = false;
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            GameObject femurBrakerButton = GameObject.Find(config.GameObjectName);

            if (femurBrakerButton != null)
            {
                Collider collider = femurBrakerButton.GetComponent<Collider>();
                if (collider != null)
                {
                    GameObject.Destroy(collider);
                }
            }
        }

        void CheckForCollider(GameObject gameObject)
        {
            MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                GameObject.Destroy(meshCollider);
                AddBoxCollider(gameObject);
            }
            else
            {
                AddBoxCollider(gameObject);
            }
        }

        void AddBoxCollider(GameObject gameObject)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();

            boxCollider.size = new Vector3(7f, 3f, 7f);
            boxCollider.center = new Vector3(0f, 0.5f, 0f);
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Door.Name.Contains("106") && is106CellLocked)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            float activationTime = ev.Generator.ActivationTime;

            if (!generatorCooldowns.ContainsKey(ev.Generator))
            {
                generatorCooldowns[ev.Generator] = activationTime;
                generatorActivationTimes[ev.Generator] = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;

                activeGeneratorsCount++;
            }

            Timing.CallDelayed(activationTime, () =>
            {
                if (generatorCooldowns.ContainsKey(ev.Generator))
                {
                    generatorCooldowns.Remove(ev.Generator);

                    if (generatorCooldowns.Count == 0 && activeGeneratorsCount >= 3)
                    {
                        if (!hasStartedProcess)
                        {
                            if (IsScp106Alive())
                            {
                                Unlock106CellDoor();
                                hasStartedProcess = true;
                            }
                        }
                    }
                }
            });
        }

        private bool IsScp106Alive()
        {
            foreach (var player in Exiled.API.Features.Player.List)
            {
                if (player.Role == RoleTypeId.Scp106 && player.IsAlive)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            if (generatorCooldowns.ContainsKey(ev.Generator))
            {
                generatorCooldowns.Remove(ev.Generator);
                activeGeneratorsCount--;
            }
        }

        public void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            if (ev.Player.IsCuffed || !ev.Player.IsAlive || ev.Player.Role.Type == RoleTypeId.Scp106)
                return;

            if (actionTriggered == true && IsLookingAtCustomModel(ev.Player))
            {
                ev.Player.ShowHint(config.FemurBreakerActivatedHint);
                return;
            }

            if (!IsScp106Alive() && IsLookingAtCustomModel(ev.Player))
            {
                ev.Player.ShowHint(config.SCP106NotAliveHint);
                return;
            }

            if (!ev.IsAllowed && IsLookingAtCustomModel(ev.Player) && IsScp106Alive() && actionTriggered == false)
            {
                actionTriggered = true;
                PlayScreamSound();
                Timing.CallDelayed(28f, KillSCP106);
            }
        }

        private bool IsLookingAtCustomModel(Player player)
        {
            float maxDistance = 5.0f;
            Vector3 origin = player.Position;
            Vector3 direction = player.ReferenceHub.PlayerCameraReference.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
            {
                if (hit.collider.gameObject.name == config.GameObjectName)
                {
                    return true;
                }
            }
            return false;
        }

        private void Lock106CellDoor()
        {
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.Scp106Primary || door.Type == DoorType.Scp106Secondary)
                {
                    door.ChangeLock(DoorLockType.Isolation);
                }
            }
        }

        private void Unlock106CellDoor()
        {
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.Scp106Primary || door.Type == DoorType.Scp106Secondary)
                {
                    door.ChangeLock(DoorLockType.None);
                }
            }
            is106CellLocked = false;
        }

        private void PlayScreamSound()
        {
            try
            {
                if (!File.Exists(config.AudioFilePath))
                {
                    return;
                }

                if (femurBreaker == null || audioPlayer == null)
                {
                    CreateFakePlayer();
                }

                audioPlayer.Enqueue(config.AudioFilePath, 0);
                audioPlayer.Play(0);
            }
            catch (Exception)
            {
            }
        }

        private void KillSCP106()
        {
            foreach (var player in Player.List)
            {
                if (player.Role == RoleTypeId.Scp106)
                {
                    player.Hurt(9999, Exiled.API.Enums.DamageType.FemurBreaker);
                }
            }
        }

        private void CreateFakePlayer()
        {
            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            var fakeConnection = new FakeConnection(9999);
            femurBreaker = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);

            Npc npc = new Npc(femurBreaker);
            npc.DisplayNickname = config.FakePlayerName;

            audioPlayer = CustomAudioPlayer.Get(femurBreaker);
        }
    }

    public class FakeConnection : NetworkConnectionToClient
    {
        public FakeConnection(int connectionId) : base(connectionId)
        {
        }

        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }

        public override void Disconnect()
        {
        }
    }
}
