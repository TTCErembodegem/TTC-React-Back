using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task <IEnumerable<Club>> Get() => await _service.GetActiveClubs();

        [HttpPost]
        [Route("UpdateClub")]
        public async Task<Club> UpdateClub([FromBody]Club club)
        {
            var result = await _service.UpdateClub(club);
            return result;
        }

        #region Club Board
        [HttpPost]
        [Route("Board")]
        public async Task SaveBoardMember([FromBody]BoardMember m)
        {
            await _service.SaveBoardMember(m.PlayerId, m.BoardFunction, m.Sort);
        }

        [HttpPost]
        [Route("Board/{playerId}")]
        public async Task DeleteBoardMember([FromUri]int playerId)
        {
            await _service.DeleteBoardMember(playerId);
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
