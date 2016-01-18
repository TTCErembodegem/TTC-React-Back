using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Ttc.DataAccess.Entities;
using Ttc.UnitTests.Players;
using Ttc.WebApi.Controllers;

namespace Ttc.UnitTests
{
    [TestClass]
    public class TestPlayersController
    {
        [TestMethod]
        public void Get_ShouldReturnAllPlayers()
        {
            var controller = new PlayersController();
            JsonResult<Speler> result = null;

            using (var x =  new TestTtcDbContext())
            {
                result = controller.Get() as JsonResult<Speler>;
            }

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));        
        }
    }
}
