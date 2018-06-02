﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telegraphy.Net;
using Telegraphy.Net.Exceptions;

namespace UnitTests.Switchboard
{
    [TestClass]
    public class LocalSwitchBoardTests
    {
        [TestMethod]
        public void VerifyRegisterOfDefaultSwitchBoardWithoutActorThrowsException()
        {
            try
            {
                Telegraph.Instance.UnRegisterAll();
                LocalOperator op = new LocalOperator();
                long operatorID = Telegraph.Instance.Register<string>(op);

                string message = "VerifyRegisterOfDefaultSwitchBoardWithoutActorThrowsException";
                Telegraph.Instance.Tell(message);
                Assert.IsTrue(false); // we expected this exception
            }
            catch(SwitchBoardRequiresARegisteredActorOrActionException ex)
            {
                Assert.IsTrue(true); // we expected this exception
            }
        }
    }
}
