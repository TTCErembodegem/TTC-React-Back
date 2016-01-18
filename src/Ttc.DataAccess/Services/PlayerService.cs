using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.DataAccess.Services
{
    public class PlayerService
    {
        public IEnumerable<Player> Get()
        {
            using (var dbContext = new TtcDbContext())
            {
                return Mapper.Map<IList<Speler>, IList<Player>>(dbContext.Players.Take(3).ToArray());
            }
        }
    }
}