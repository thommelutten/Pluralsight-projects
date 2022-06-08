using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(c => c.VenueName, o => o.MapFrom(m => m.Location.VenueName))
                .ReverseMap();

            CreateMap<Talk, TalkModel>()
                .ReverseMap();

            CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();
        }
    }
}
