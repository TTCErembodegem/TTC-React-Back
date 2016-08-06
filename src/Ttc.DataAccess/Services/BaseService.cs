using AutoMapper;

namespace Ttc.DataAccess.Services
{
    public class BaseService
    {
        protected IMapper Mapper
        {
            get { return AutoMapperConfig.Factory.CreateMapper(); }
        }

        public BaseService()
        {
            
        }
    }
}