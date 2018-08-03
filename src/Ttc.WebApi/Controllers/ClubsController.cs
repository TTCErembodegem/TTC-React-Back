using System.Collections.Generic;
using System.Web.Http;
using Ttc.DataAccess.Services;
using Ttc.Model.Clubs;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers
{
    [RoutePrefix("api/clubs")]
    public class ClubsController : BaseController
    {
        #region Constructor
        private readonly ClubService _service;

        public ClubsController(ClubService service)
        {
            _service = service;
        }
        #endregion

        [AllowAnonymous]
        public IEnumerable<Club> Get() => _service.GetActiveClubs();

        #region Club Board
        [HttpPost]
        [Route("Board")]
        public void SaveBoardMember([FromBody]BoardMember m)
        {
            _service.SaveBoardMember(m.PlayerId, m.BoardFunction, m.Sort);
        }

        [HttpPost]
        [Route("Board/{playerId}")]
        public void DeleteBoardMember([FromUri]int playerId)
        {
            _service.DeleteBoardMember(playerId);
        }

        public class BoardMember
        {
            public int PlayerId { get; set; }
            public string BoardFunction { get; set; }
            public int Sort { get; set; }

            public override string ToString() => $"{PlayerId} => {BoardFunction} ({Sort})";
        }
        #endregion
    }
}
