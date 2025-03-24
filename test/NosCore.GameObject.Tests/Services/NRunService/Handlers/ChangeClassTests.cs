﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Data.Enumerations;
using NosCore.GameObject.ComponentEntities.Interfaces;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.GameObject.Services.InventoryService;
using NosCore.GameObject.Services.ItemGenerationService;
using NosCore.GameObject.Services.NRunService;
using NosCore.GameObject.Services.NRunService.Handlers;
using NosCore.Packets.ClientPackets.Npcs;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.Chats;
using NosCore.Packets.ServerPackets.UI;
using NosCore.Shared.Enumerations;
using NosCore.Tests.Shared;
using Serilog;

//TODO stop using obsolete
#pragma warning disable 618

namespace NosCore.GameObject.Tests.Services.NRunService.Handlers
{
    [TestClass]
    public class ChangeClassTests
    {
        private static readonly ILogger Logger = new Mock<ILogger>().Object;

        private IItemGenerationService? _item;
        private NrunService _nrRunService = null!;
        private ClientSession? _session;

        [TestInitialize]
        public async Task SetupAsync()
        {
            await TestHelpers.ResetAsync().ConfigureAwait(false);
            _session = await TestHelpers.Instance.GenerateSessionAsync().ConfigureAwait(false);
            _item = TestHelpers.Instance.GenerateItemProvider();
            _nrRunService = new NrunService(
                new List<IEventHandler<Tuple<IAliveEntity, NrunPacket>, Tuple<IAliveEntity, NrunPacket>>>
                    {new ChangeClassEventHandler(Logger, TestHelpers.Instance.LogLanguageLocalizer)});
        }

        [DataTestMethod]
        [DataRow((int)CharacterClassType.Archer)]
        [DataRow((int)CharacterClassType.Mage)]
        [DataRow((int)CharacterClassType.Swordsman)]
        public async Task UserCantChangeClassLowLevelAsync(int characterClassInt)
        {
            var characterClass = (CharacterClassType)characterClassInt;
            _session!.Character.Level = 15;
            await _nrRunService.NRunLaunchAsync(_session, new Tuple<IAliveEntity, NrunPacket>(_session.Character, (new NrunPacket
            {
                VisualType = VisualType.Npc,
                Runner = NrunRunnerType.ChangeClass,
                VisualId = 0,
                Type = (byte)characterClass
            })));

            var msgiPacket = (MsgiPacket?)_session.LastPackets.FirstOrDefault(s => s is MsgiPacket);
            Assert.IsTrue(msgiPacket?.Type == MessageType.Default && msgiPacket?.Message == Game18NConstString.CanNotChangeJobAtThisLevel);
        }

        [DataTestMethod]
        [DataRow((int)CharacterClassType.Archer)]
        [DataRow((int)CharacterClassType.Mage)]
        [DataRow((int)CharacterClassType.Swordsman)]
        public async Task UserCantChangeClassLowJobLevelAsync(int characterClassInt)
        {
            var characterClass = (CharacterClassType)characterClassInt;
            _session!.Character.JobLevel = 20;
            await _nrRunService.NRunLaunchAsync(_session, new Tuple<IAliveEntity, NrunPacket>(_session.Character, (new NrunPacket
            {
                VisualType = VisualType.Npc,
                Runner = NrunRunnerType.ChangeClass,
                VisualId = 0,
                Type = (byte)characterClass
            })));
         
            var msgiPacket = (MsgiPacket?)_session.LastPackets.FirstOrDefault(s => s is MsgiPacket);
            Assert.IsTrue(msgiPacket?.Type == MessageType.Default && msgiPacket?.Message == Game18NConstString.CanNotChangeJobAtThisLevel);
        }

        [DataTestMethod]
        [DataRow((int)CharacterClassType.Archer)]
        [DataRow((int)CharacterClassType.Mage)]
        [DataRow((int)CharacterClassType.Swordsman)]
        public async Task UserCantChangeBadClassAsync(int characterClassInt)
        {
            var characterClass = (CharacterClassType)characterClassInt;
            _session!.Character.Class = characterClass;
            await _nrRunService.NRunLaunchAsync(_session, new Tuple<IAliveEntity, NrunPacket>(_session.Character, (new NrunPacket
            {
                VisualType = VisualType.Npc,
                Runner = NrunRunnerType.ChangeClass,
                VisualId = 0,
                Type = (byte)CharacterClassType.Swordsman
            })));
            var packet = (MsgPacket?)_session.LastPackets.FirstOrDefault(s => s is MsgPacket);
        }

        [DataTestMethod]
        [DataRow((int)CharacterClassType.MartialArtist)]
        [DataRow((int)CharacterClassType.Adventurer)]
        public async Task UserCantChangeToBadClassAsync(int characterClassInt)
        {
            var characterClass = (CharacterClassType)characterClassInt;
            _session!.Character.Level = 15;
            _session.Character.JobLevel = 20;
            await _nrRunService.NRunLaunchAsync(_session, new Tuple<IAliveEntity, NrunPacket>(_session.Character, (new NrunPacket
            {
                VisualType = VisualType.Npc,
                Runner = NrunRunnerType.ChangeClass,
                VisualId = 0,
                Type = (byte)characterClass
            })));
            
            Assert.IsTrue((_session.Character.Class == CharacterClassType.Adventurer) &&
                (_session.Character.Level == 15) &&
                (_session.Character.JobLevel == 20));
        }

        [DataTestMethod]
        [DataRow((int)CharacterClassType.Archer)]
        [DataRow((int)CharacterClassType.Mage)]
        [DataRow((int)CharacterClassType.Swordsman)]
        public async Task UserCanChangeClassAsync(int characterClassInt)
        {
            var characterClass = (CharacterClassType)characterClassInt;
            _session!.Character.Level = 15;
            _session.Character.JobLevel = 20;
            await _nrRunService.NRunLaunchAsync(_session, new Tuple<IAliveEntity, NrunPacket>(_session.Character, (new NrunPacket
            {
                VisualType = VisualType.Npc,
                Runner = NrunRunnerType.ChangeClass,
                VisualId = 0,
                Type = (byte)characterClass
            })));

            Assert.IsTrue((_session.Character.Class == characterClass) && (_session.Character.Level == 15) &&
                (_session.Character.JobLevel == 1));
        }

        [DataTestMethod]
        [DataRow((int)CharacterClassType.Archer)]
        [DataRow((int)CharacterClassType.Mage)]
        [DataRow((int)CharacterClassType.Swordsman)]
        public async Task UserCanNotChangeClassWhenEquipmentAsync(int characterClassInt)
        {
            var characterClass = (CharacterClassType)characterClassInt;
            _session!.Character.Level = 15;
            _session.Character.JobLevel = 20;
            _session.Character.InventoryService!.AddItemToPocket(InventoryItemInstance.Create(_item!.Create(1, 1), 0));
            var item = _session.Character.InventoryService.First();
            item.Value.Type = NoscorePocketType.Wear;
            await _nrRunService.NRunLaunchAsync(_session, new Tuple<IAliveEntity, NrunPacket>(_session.Character, (new NrunPacket
            {
                VisualType = VisualType.Npc,
                Runner = NrunRunnerType.ChangeClass,
                VisualId = 0,
                Type = (byte)characterClass
            })));

            var packet = (SayiPacket?)_session.LastPackets.FirstOrDefault(s => s is SayiPacket);
            Assert.IsTrue(packet?.VisualType == VisualType.Player && packet?.VisualId == _session.Character.CharacterId && packet?.Type == SayColorType.Yellow && packet?.Message == Game18NConstString.RemoveEquipment);
        }
    }
}