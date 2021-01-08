﻿using BlogSystemAPI.Data;
using BlogSystemAPI.Models.Database;
using BlogSystemAPI.Models.Inputs;
using BlogSystemAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogSystemAPI.Repository
{
    public class PostRepository : IPostRepository
    {
        private BlogDBContext _dbContext;

        public PostRepository(BlogDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Delete(int id)
        {
            Post post = await _dbContext.Posts.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (post != null)
            {
                _dbContext.Posts.Remove(post);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> GetAll()
        {
            return await _dbContext.Posts.ToListAsync();
        }

        public async Task<Post> GetById(int id)
        {
            return await _dbContext.Posts.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Post>> GetFilteredList(FilterInput filters)
        {
            var posts = await (from p in _dbContext.Posts
                               where 1 == 1
                               && (filters.StartDate.HasValue ? filters.StartDate <= p.CreateDate : true)
                               && (filters.EndDate.HasValue ? filters.EndDate >= p.CreateDate : true)
                               && (!string.IsNullOrEmpty(filters.ContentContains) ? p.Content.Contains(filters.ContentContains) : true)
                               && (filters.UserId.HasValue ? filters.UserId == p.UserId : true)
                               select new Post
                               {
                                   Content = p.Content,
                                   CreateDate = p.CreateDate,
                                   Id = p.Id,
                                   UpdateDate = p.UpdateDate,
                                   UserId = p.UserId
                               }).ToListAsync();

            if (filters.OrderByCreateDateDesc)
            {
                posts = posts.OrderByDescending(p => p.CreateDate).ToList();
            }

            return posts;
        }

        public async Task<Post> Insert(Post entity)
        {
            await _dbContext.Posts.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.Posts.Where(x => x.Content == entity.Content && x.UserId == entity.UserId).OrderByDescending(y => y.Id).FirstOrDefaultAsync();
        }

        public async Task Update(Post entity)
        {
            Post post = await _dbContext.Posts.Where(x => x.Id == entity.Id).FirstOrDefaultAsync();

            if (post == null)
                return;

            if (post.Content != entity.Content)
                post.Content = entity.Content;

            if (post.UserId != entity.UserId)
                post.UserId = entity.UserId;

            post.UpdateDate = DateTime.Now;

            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();

        }
    }
}
