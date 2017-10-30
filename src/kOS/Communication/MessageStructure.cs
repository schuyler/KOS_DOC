﻿using System;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.Safe;
using kOS.Serialization;
using kOS.Safe.Serialization;
using kOS.Safe.Exceptions;

namespace kOS.Communication
{
    [kOS.Safe.Utilities.KOSNomenclature("Message")]
    public class MessageStructure : SerializableStructure, IHasSharedObjects
    {
        private static string DumpMessage = "message";

        public Message Message { get; private set; }
        private SharedObjects shared;

        public SharedObjects Shared
        {
            set
            {
                shared = value;
            }
        }

        public MessageStructure()
        {
            InitializeSuffixes();
        }

        public MessageStructure(Message message, SharedObjects shared)
        {
            Message = message;
            this.shared = shared;

            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("SENTAT", new Suffix<kOS.Suffixed.TimeSpan>(() => new kOS.Suffixed.TimeSpan(Message.SentAt)));
            AddSuffix("RECEIVEDAT", new Suffix<kOS.Suffixed.TimeSpan>(() => new kOS.Suffixed.TimeSpan(Message.ReceivedAt)));
            AddSuffix("SENDER", new Suffix<Structure>(GetVesselTarget));
            AddSuffix("HASSENDER", new Suffix<BooleanValue>(GetVesselExists));
            AddSuffix("CONTENT", new Suffix<Structure>(DeserializeContent));
        }

        public Vessel GetVessel()
        {
            return (FlightGlobals.Vessels.Find((v) => v.id.ToString().Equals(Message.Vessel)));
        }

        public Structure GetVesselTarget()
        {
            Vessel vessel = GetVessel();

            if (vessel == null)
            {
                return new BooleanValue(false);
            }

            return VesselTarget.CreateOrGetExisting(vessel, shared);
        }
        
        public BooleanValue GetVesselExists()
        {
            return new BooleanValue((GetVessel() != null));
        }

        public Structure DeserializeContent()
        {
            if (Message.Content is Dump)
            {
                return new SerializationMgr(shared).CreateFromDump(Message.Content as Dump) as SerializableStructure;
            }

            return Structure.FromPrimitiveWithAssert(Message.Content);
        }

        public override string ToString()
        {
            return "Message(" + Message.Vessel.ToString() + ")";
        }

        public override Dump Dump()
        {
            Dump dump = new DumpWithHeader
            {
                Header = "Message"
            };

            dump.Add(DumpMessage, Message);

            return dump;
        }

        public override void LoadDump(Dump dump)
        {
            Message = dump[DumpMessage] as Message;
        }
    }
}

