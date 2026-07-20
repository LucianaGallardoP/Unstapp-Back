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
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<CreatePostDto, Post>()
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.MediaUrl, opt => opt.Ignore())
                .ForMember(dest => dest.PostDate, opt => opt.Ignore())
                .ForMember(dest => dest.Likes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.PostCareers, opt => opt.Ignore());

            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.User.Name} {src.User.LastName}"))
                .ForMember(dest => dest.UserAvatarUrl,
                    opt => opt.MapFrom(src => src.User.AvatarUrl))
                .ForMember(dest => dest.LikesCount,
                    opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.CommentsCount,
                    opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.isImportant,
                    opt => opt.MapFrom(src => src.IsImportant));
        }
    }
}
