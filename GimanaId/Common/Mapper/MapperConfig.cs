﻿using AutoMapper;
using GimanaIdApi.DTOs.Request;
using GimanaIdApi.DTOs.Response;
using DomainModel.ValueObjects;
using DomainModel.Entities;

namespace GimanaIdApi.Common.Mapper
{
    public class MapperConfig
    {
        public MapperConfiguration GetConfiguration()
        {
            return new MapperConfiguration(
                (config) =>
                {
                    //config.CreateMap<ArticleHistory, ArticleHistoryDto>();

                    //config.CreateMap<ArticleIssue, ArticleIssueDto>();
                    
                    //config.CreateMap<ArticleRating, ArticleRatingDto>();
                    
                    config.CreateMap<ArticlePart, ArticlePartDto>();
                    
                    config.CreateMap<ArticleStep, ArticleStepDto>();
                    
                    config.CreateMap<AuthToken, AuthTokenDto>();
                    
                    config.CreateMap<Article, DetailedArticleDto>()
                    .ForMember(
                        dto => dto.Contributors, 
                        exp => exp.MapFrom(ori => ori.Users));
                    
                    config.CreateMap<User, DetailedUserDto>()
                    .ForMember(
                        dto => dto.ContributedArticles,
                        exp => exp.MapFrom(ori => ori.Articles));
                    
                    config.CreateMap<Image, ImageDto>()
                    .ForMember(
                        dto => dto.Base64EncodedData,
                        exp => exp.MapFrom(ori => ori.Base64EncodedData == null ? "" : ori.Base64EncodedData));
                    
                    config.CreateMap<Article, SimpleArticleDto>();
                    
                    config.CreateMap<User, SimpleUserDto>();
                    
                    config.CreateMap<Email, UserEmailDto>();
                    
                    config.CreateMap<User, UserIdDto>();
                    
                    config.CreateMap<UserPrivilege, UserPrivilegeDto>();
                    
                    config.CreateMap<CreateImageDto, Image>();

                    config.CreateMap<CreateArticleDto, Article>();
                    
                    //config.CreateMap<CreateArticleStepDto, ArticleIssue>();
                    
                    config.CreateMap<CreateArticlePartDto, ArticlePart>();
                    
                    config.CreateMap<CreateArticleStepDto, ArticleStep>();
                });
        }
    }
}
