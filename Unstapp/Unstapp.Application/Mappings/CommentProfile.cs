using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Application.Mappings
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.User.Name} {src.User.LastName}"))
                .ForMember(dest => dest.AvatarUrl,
                    opt => opt.MapFrom(src => src.User.AvatarUrl));

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.CommentId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore());
        }
    }
}
