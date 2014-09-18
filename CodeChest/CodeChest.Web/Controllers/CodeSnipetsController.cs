﻿namespace CodeChest.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Microsoft.AspNet.Identity;

    using CodeChest.Data;
    using CodeChest.Models;
    using CodeChest.Web.Infrastructure;
    using CodeChest.Web.DataModels;

    public class CodeSnipetsController : BaseApiController
    {
        private const string NO_CODE_SNIPET = "Code snippet does not exist or has been deleted!";
        private const string NOT_YOUR_SNIPET = "You can not edit/delete someone else's code snippet!";
        //TODO: konstantite da se iznesat v otdelen klas/enum ili kakto e kulturno

        public CodeSnipetsController(ICodeChestData data, IUserIdProvider userIdProvider)
            : base(data, userIdProvider)
        {
        }

        [HttpGet]
        public IHttpActionResult All()
        {
            //TODO: filter by language/title; get n pages
            var codeSnipetTitles = this.data
                .CodeSnipets
                .All()
                .Select(CodeSnipetsPartialDataModel.FromCodeSnipet);

            return Ok(codeSnipetTitles);
        }

        [HttpGet]
        public IHttpActionResult ById(int id)
        {
            var codeSnipet = this.data
                .CodeSnipets
                .All()
                .Where(c => c.Id == id)
                .Select(CodeSnipetDataModel.FromCodeSnipet)
                .FirstOrDefault();

            if (codeSnipet == null)
            {
                return BadRequest(NO_CODE_SNIPET);
            }

            codeSnipet.Score = CalculateScoreForSnipet(codeSnipet.Id);
            return Ok(codeSnipet);
        }

        [Authorize]
        [HttpPost]
        public IHttpActionResult Create(CodeSnipetDataModel codeSnipet)
        {
            if (!this.ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = this.userIdProvider.GetUserId();
            var newCodeSnipet = new CodeSnipet
            {
                Content = codeSnipet.Content,
                Language = codeSnipet.Language,
                Title = codeSnipet.Title,
                UserId = currentUserId,
                AddedOn = DateTime.Now
            };

            this.data.CodeSnipets.Add(newCodeSnipet);
            this.data.SaveChanges();

            UpdateLastActivityForUser();

            return Ok(newCodeSnipet.Id);
        }

        [Authorize]
        [HttpPut]
        public IHttpActionResult Update(int id, CodeSnipetDataModel codeSnipet)
        {
            if (!this.ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCodeSnipet = this.data.CodeSnipets.All().FirstOrDefault(a => a.Id == id);
            if (existingCodeSnipet == null)
            {
                return BadRequest(NO_CODE_SNIPET);
            }

            var currentUserId = this.userIdProvider.GetUserId();
            if (existingCodeSnipet.UserId != currentUserId)
            {
                return BadRequest(NOT_YOUR_SNIPET);
            }

            //TODO: maybe add a field modifiedOn in the database and change it when updating a snippet
            existingCodeSnipet.Content = codeSnipet.Content;
            existingCodeSnipet.Language = codeSnipet.Language;
            existingCodeSnipet.Title = codeSnipet.Title;
            this.data.SaveChanges();

            codeSnipet.Id = id;
            codeSnipet.UserId = existingCodeSnipet.UserId;
            codeSnipet.AddedOn = existingCodeSnipet.AddedOn;
            codeSnipet.Score = CalculateScoreForSnipet(id);

            UpdateLastActivityForUser();

            return Ok(codeSnipet);
        }

        [Authorize]
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var existingCodeSnipet = this.data.CodeSnipets.All().FirstOrDefault(a => a.Id == id);
            if (existingCodeSnipet == null)
            {
                return BadRequest(NO_CODE_SNIPET);
            }

            var currentUserId = this.userIdProvider.GetUserId();
            if (existingCodeSnipet.UserId != currentUserId)
            {
                return BadRequest(NOT_YOUR_SNIPET);
            }

            this.data.CodeSnipets.Delete(existingCodeSnipet);
            this.data.SaveChanges();

            UpdateLastActivityForUser();

            return Ok();
        }

        private double? CalculateScoreForSnipet(int id)
        {
            var ratings = this.data.Ratings
                .All()
                .Where(r => r.CodeSnipetId == id)
                .Select(RatingDataModel.FromCodeSnipet);

            double sum = 0;
            double? score = null;

            if (ratings.Count() > 0)
            {
                foreach (var rating in ratings)
                {
                    sum += rating.Score;
                }

                score = sum / ratings.Count();
            }

            return score;
        }
    }
}