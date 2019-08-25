﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using mentula_manducare.Classes;
using mentula_manducare.Enums;

namespace mentula_manducare.Objects
{
    public class PlayerContainer
    {
        [ScriptIgnore]
        public MemoryHandler Memory;

        public int PlayerIndex;
        private int resolvedStaticIndex;
        private int resolvedDynamicIndex;
        public PlayerContainer(MemoryHandler Memory, int PlayerIndex)
        {
            this.Memory = Memory;
            this.PlayerIndex = PlayerIndex;
            var name_ = this.Name;
            for (var i = 0; i < 16; i++)
            {
                if (Memory.ReadStringUnicode(0x530E4C + (i * 0x128), 16, true) == name_)
                    resolvedStaticIndex = i;
                if (Memory.ReadStringUnicode(0x30002708 + (i * 0x204), 16) == name_)
                    resolvedDynamicIndex = i;
            }
        }

        public string Name
        {
            get => Memory.ReadStringUnicode(0x9917DA + (PlayerIndex * 0x40), 16, true);
            set => Memory.WriteStringUnicode(0x9917DA + (PlayerIndex * 0x40), value, true);
        }

        public Team Team
        {
            get => (Team)Memory.ReadByte(0x530F4C + (resolvedStaticIndex * 0x128), true);
            set => Memory.WriteByte(0x530F4C + (resolvedStaticIndex * 0x128), (byte) value, true);
        }

        public Biped Biped
        {
            get => (Biped) Memory.ReadByte(0x3000274C + (resolvedDynamicIndex * 0x204));
            set => Memory.WriteByte(0x3000274C + (resolvedDynamicIndex * 0x204), (byte) value);
        }

        public byte BipedPrimaryColor
            => Memory.ReadByte(0x530E8C + (resolvedStaticIndex * 0x128), true);

        public byte BipedSecondaryColor
            => Memory.ReadByte(0x530E8D + (resolvedStaticIndex * 0x128), true);

        public byte PrimaryEmblemColor
            => Memory.ReadByte(0x530E8E + (resolvedStaticIndex * 0x128), true);

        public byte SecondaryEmblemColor
            => Memory.ReadByte(0x530E8F + (resolvedStaticIndex * 0x128), true);

        public byte EmblemForeground
            => Memory.ReadByte(0x530E91 + (resolvedStaticIndex * 0x128), true);

        public byte EmblemBackground
            => Memory.ReadByte(0x530E92 + (resolvedStaticIndex * 0x128), true);

        public byte EmblemToggle
            => (byte) (Memory.ReadByte(0x530E93 + (resolvedStaticIndex * 0x128), true) == 0 ? 1 : 0);

        public string EmblemURL =>
            $"http://halo.bungie.net/Stats/emblem.ashx?s=120&0={BipedPrimaryColor.ToString()}&1={BipedSecondaryColor.ToString()}&2={PrimaryEmblemColor.ToString()}&3={SecondaryEmblemColor.ToString()}&fi={EmblemForeground.ToString()}&bi={EmblemBackground.ToString()}&fl={EmblemToggle.ToString()}";

        public int NetworkIdentifier =>
            Memory.ReadByte(0x530E3C + (resolvedStaticIndex * 0x128), true) - 1;

        public int IPHex =>
            Memory.ReadInt(0x5321DC + (NetworkIdentifier * 0x10C), true);

        public async void TimeoutPlayer()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < 16; i++)
                {
                    var networkObjectIP = Memory.ReadInt(0x526574 + (i * 0x740), true);
                    if (networkObjectIP != IPHex) continue;
                    var startTime = DateTime.UtcNow;
                    while(DateTime.Now - startTime < TimeSpan.FromSeconds(20))
                        Memory.WriteByte(0x5265CE + (i * 0x740), 0, true);
                    break;
                }
            });
        } 
    }
}
