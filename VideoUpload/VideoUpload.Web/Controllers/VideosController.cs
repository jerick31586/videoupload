﻿using Microsoft.AspNet.Identity;
using NReco.VideoConverter;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoUpload.Core.Entities;
using VideoUpload.EF;
using VideoUpload.Web.Common;
using VideoUpload.Web.Models;
using VideoUpload.Web.Models.Identity;

namespace VideoUpload.Web.Controllers
{
    [RoutePrefix("videos")]
    public class VideosController : AppController
    {
        private readonly UnitOfWork _uow;
        private readonly UserManager _mgr;

        public VideosController(UnitOfWork unitOfWork, UserManager mgr)
        {
            _uow = unitOfWork;
            _mgr = mgr;
        }
                
        [AccessActionFilter(Type= "Video", Value ="CanRead")]
        public async Task<ActionResult> Index(int? page)
        {
            //var viewModel = new PostViewModel(_uow);
            //viewModel.Posts.ToPagedList(page ?? 1, 1);   
                     
            var posts = await _uow.Posts.GetAllAsync();

            ViewBag.ApprovedVideos = posts.Where(post => post.HasApproval && post.IsApproved).OrderBy(d => d.DateApproved).Take(5).ToList();
            ViewBag.VideosPlayed = posts.Where(post => post.HasPlayedVideo).OrderBy(d => d.DatePlayedVideo).Take(5).ToList().ToList();

            var viewModel = new List<PostViewModel>();

            posts.ForEach(x =>
            {
                var attachments = x.Attachments.OrderBy(y => y.AttachmentNo).ToList();

                var attachment = attachments.FirstOrDefault();

                if (attachment == null)
                {
                    attachments = null;
                }

                viewModel.Add(new PostViewModel
                {
                    PostID = x.PostID,
                    PlateNumber = x.PlateNumber,
                    Description = x.Description,
                    UploadedBy = x.User.UserName,
                    Attachments = attachments,
                    DateUploaded = x.DateUploaded,
                    EditedBy = x.EditedBy,
                    DateEdited = x.DateEdited,
                    HasApproval = x.HasApproval,
                    IsApproved = x.IsApproved,
                    BranchName = x.Branch.BranchName                  
                });
            });
            viewModel = viewModel.OrderByDescending(x => x.DateUploaded).ToList();

            return View(viewModel.ToPagedList(page ?? 1, 3));
        }
        
        [Route("search")]
        public async Task<ActionResult> Search(string q)
        {
            var posts = await _uow.Posts.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                posts = posts.Where(query => query.PlateNumber.Contains(q) || query.Description.Contains(q)).ToList();
            }
            var viewModel = new List<PostViewModel>();

            posts.ForEach(x => 
            {
                var attachments = x.Attachments.OrderBy(y => y.AttachmentNo).ToList();

                var attachment = attachments.FirstOrDefault();

                if (attachment == null)
                {
                    attachments = null;
                }

                viewModel.Add(new PostViewModel
                {
                    PostID = x.PostID,
                    PlateNumber = x.PlateNumber,
                    Description = x.Description,
                    UploadedBy = x.User.UserName,
                    Attachments = attachments,
                    DateUploaded = x.DateUploaded,
                    EditedBy = x.EditedBy,
                    DateEdited = x.DateEdited,
                    HasApproval = x.HasApproval,
                    IsApproved = x.IsApproved
                });
            });

            return View(viewModel);
        }

        [Route("upload")]
        [AccessActionFilter(Type = "Video", Value = "CanCreate")]
        public ActionResult Upload()
        {
            var post = new CreatePostViewModel();            
            return View(post);            
        }

        [HttpPost]
        [Route("upload")]
        [AccessActionFilter(Type = "Video", Value = "CanCreate")]
        public async Task<ActionResult> Upload(CreatePostViewModel viewModel)
        {
            var success = false;

            if (ModelState.IsValid)
            {
                var countOfAttachments = 0;
                
                var contentTypeArray = new string[] 
                {
                    "video/mp4"
                    //"video/avi",
                    //"application/x-mpegURL",
                    //"video/MP2T",
                    //"video/3gpp",
                    //"video/quicktime",
                    //"video/x-msvideo",
                    //"video/x-ms-wmv"
                };

                var post = new Post
                {
                    PlateNumber = viewModel.PlateNumber,
                    Description = viewModel.Description,
                    UserID = User.Identity.GetUserId(),
                    DateUploaded = viewModel.DateUploaded,
                    BranchID = CurrentUser.BranchID
                };

                foreach (var item in viewModel.Attachments)
                {
                    if (item != null)
                    {
                        if (!contentTypeArray.Contains(item.ContentType))
                        {                            
                            ModelState.AddModelError("", "video file must be an mp4 format");

                            return Json(new { success = success, message = "Video file must be an mp4 format" });
                        }
                        countOfAttachments++;                        

                        var ext = Path.GetExtension(item.FileName);

                        var path = Server.MapPath("~/Uploads/Videos");
                        var thumbnailPath = Server.MapPath("~/Uploads/Thumbnails");
                     
                        //create new entity for each attachment
                        var attachment = new PostAttachment();

                        attachment.PostAttachmentID = Guid.NewGuid().ToString();
                        attachment.FileName = attachment.PostAttachmentID + ext;
                        attachment.MIMEType = item.ContentType;
                        attachment.FileSize = item.ContentLength;
                        attachment.FileUrl = path + "/" + attachment.FileName;
                        attachment.DateCreated = viewModel.DateUploaded;
                        attachment.AttachmentNo = $"Attachment {countOfAttachments.ToString()}";
                        attachment.ThumbnailFileName = attachment.PostAttachmentID + ".jpeg";
                        attachment.ThumbnailUrl = thumbnailPath + attachment.ThumbnailFileName;
                      
                        var fileUrlToConvert = Path.Combine(path, attachment.FileName);                        

                        using (var fileStream = System.IO.File.Create(fileUrlToConvert))
                        {
                            var stream = item.InputStream;
                            stream.CopyTo(fileStream);
                        }

                        //TODO: uncomment this in production 
                        //var ffMpeg = new FFMpegConverter();
                        //ffMpeg.FFMpegToolPath = path; //need to have this and upload the ffmpeg.exe to this path;                        

                        var file = new FileInfo(fileUrlToConvert);

                        if (file.Exists)
                        {
                            //TODO: uncomment this in production
                            //ffMpeg.GetVideoThumbnail(fileUrlToConvert, thumbnailPath + "/" + attachment.ThumbnailFileName);
                            
                            //add the attachment to post entity
                            post.Attachments.Add(attachment);                           
                        }
                    }                
                }
                var attached = post.Attachments.FirstOrDefault();
                if (attached != null)
                {
                    _uow.Posts.Add(post);
                    await _uow.SaveChangesAsync();

                    //TOD: uncomment this in production
                    //ALERT THE SERVICE MANAGER
                    //await _mgr.CustomSendEmailAsync(User.Identity.GetUserId(), "Video upload", User.Identity.Name + " has uploaded a new video", );
                    success = true;

                    return Json(new { success = success, message = "Uploaded successfully" });                   
                }
                ModelState.AddModelError("", "Attached has not been succesfully uploaded");                
            }
            return Json(new { success = success, message = "Something went wrong. Please try again" });         
        }

        [Route("{userName}/myposts")]
        public ActionResult MyPosts(int? page)
        {
            var viewModel = new PostViewModel(_uow, User.Identity.GetUserId(), page, 15);
            
            return View(viewModel);
        }

        [Route("{userName}/{postID}")]
        [AccessActionFilter(Type = "Video", Value = "CanUpdate")]
        public async Task<ActionResult> Edit(string userName, int postID)
        {
            if (postID < 0) return View("_ResourceNotFound");

            //prevent typing directly the other's username 
            if (userName != User.Identity.Name) return View("_ResourceNotFound");

            var post = await _uow.Posts.GetByUserIDAndPostIDAsync(User.Identity.GetUserId(), postID);

            if (post == null) return View("_ResourceNotFound");            
            
            var viewModel = new PostViewModel
            {
                PostID = post.PostID,
                PlateNumber = post.PlateNumber,
                Description = post.Description,
                UploadedBy = post.User.UserName
            };
                        
            return View(viewModel);
        }

        [HttpPost]
        [Route("{userName}/{postID}")]
        [AccessActionFilter(Type = "Video", Value = "CanUpdate")]
        public async Task<ActionResult> Edit(PostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var post = await _uow.Posts.GetByIdAsync(viewModel.PostID);

                post.PlateNumber = viewModel.PlateNumber;
                post.Description = viewModel.Description;
                
                await _uow.SaveChangesAsync();
                return RedirectToAction("index");
            }
            return View(viewModel);
        }

        [Route("{postID}/{plateNumber}/{fileName}/details")]
        public async Task<ActionResult> Details(int postID, string plateNumber, string fileName)
        {            
            var post = await _uow.Posts.GetByIdAsync(postID);

            ViewBag.ApprovedVideos = await _uow.Posts.GetPostByApprovedAsync(5);
            ViewBag.VideosPlayed = await _uow.Posts.GetPostByVideoPlayedAsync(5);            

            if (post == null)
            {
                return View("_ResourceNotFound");
            }
            
            var attachment = post.Attachments.FirstOrDefault(x => x.FileName == fileName);
            
            ViewBag.FileName = attachment.FileName;
            ViewBag.MIMEType = attachment.MIMEType;
            ViewBag.AttachmentNo = attachment.AttachmentNo;

            //for some reason i need to orderby the attachment
            var attachments = post.Attachments.OrderBy(x => x.AttachmentNo).ToList();

            var viewModel = new PostViewModel
            {
                PostID = post.PostID,
                PlateNumber = post.PlateNumber,
                Description = post.Description,
                UploadedBy = post.User.UserName,
                Attachments = attachments,
                HasApproval = post.HasApproval,
                IsApproved = post.IsApproved                                
            };            
            return View(viewModel);
        }

        [AllowAnonymous]
        public ActionResult VideoResult(string fileName)
        {
            var file = _uow.Attachments.GetByFileName(fileName);

            return new CustomResult(fileName);
            //return File(file.FileUrl, file.MIMEType, file.FileName);
        }
       
        public ActionResult Download(string fileName)
        {
            return new DownloadResult(fileName);
        }

        [AccessActionFilter(Type = "Video", Value = "CanSend")]
        public async Task<ActionResult> Send(int p, string v, string sendingType)
        {
            var post = await _uow.Posts.GetByIdAsync(p);

            if (post == null)
            {
                return View("_ResourceNotFound");
            }

            var attachment = post.Attachments.FirstOrDefault(x => x.FileName == v);

            ViewBag.FileName = attachment.FileName;
            ViewBag.MIMEType = attachment.MIMEType;
            ViewBag.AttachmentNo = attachment.AttachmentNo;

            //for some reason i need to orderby the attachment
            var attachments = post.Attachments.OrderBy(x => x.AttachmentNo).ToList();

            var viewModel = new PostViewModel
            {
                PostID = post.PostID,
                PlateNumber = post.PlateNumber,
                Description = post.Description,
                UploadedBy = post.User.UserName,
                Attachments = attachments
            };
            ViewBag.SendingType = sendingType;
            return View(viewModel);
        }

        [HttpPost]
        [AccessActionFilter(Type = "Video", Value = "CanSend")]
        public  async Task<ActionResult> Send(string sendingType, string email, string subject,int p, string c, string v, string mobile)
        {                        
            var url = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action("watch", new { p = p, c = c, v = v });
            var id = User.Identity.GetUserId();

            var history = new History();
            history.Sender = id;
            history.DateSent = DateTime.UtcNow;

            if (sendingType == "email")
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject))
                {
                    return View("_Error");
                }
                history.Type = sendingType;
                history.Recipient = email;
                await _mgr.CustomSendEmailAsync(id, subject, "Watch the link for your car: " + url, email, CurrentUser.EmailPass);                                
            }
            else
            {
                history.Type = sendingType;
                history.Recipient = mobile;
                var result = await _mgr.OoredooSendSmsAsync(mobile, subject + " " + url);
                TempData["smsResult"] = result;
            }
            _uow.Histories.Add(history);
            await _uow.SaveChangesAsync();
            return RedirectToAction("index");
        }        

        [AllowAnonymous]
        [Route("{p}/{c}/{v}/watch")]
        public ActionResult Watch(int p, string c, string v)
        {
            var post = _uow.Posts.GetById(p);

            if (post == null)
            {
                return View("_ErrorWatch");
            }

            var attachment = post.Attachments.FirstOrDefault(x => x.FileName == v);

            ViewBag.FileName = attachment.FileName;
            ViewBag.MIMEType = attachment.MIMEType;
            ViewBag.AttachmentNo = attachment.AttachmentNo;

            //for some reason i need to orderby the attachment
            var attachments = post.Attachments.OrderBy(x => x.AttachmentNo).ToList();

            var viewModel = new PostViewModel
            {
                PostID = post.PostID,
                PlateNumber = post.PlateNumber,
                Description = post.Description,
                Attachments = attachments,
                UploadedBy = post.User.UserName,
                DateUploaded = post.DateUploaded,
                HasPlayedVideo = post.HasPlayedVideo,
                DatePlayedVideo = post.DatePlayedVideo
            };
            return View(viewModel);
        }

        [HttpPost]
        [AccessActionFilter(Type = "Approval", Value = "CanApproveVideo")]
        public async Task<ActionResult> Approval(bool isapproved, string postID)
        {
            var _postID = int.Parse(postID);
            var post = await _uow.Posts.GetByIdAsync(_postID);


            if (post == null)
            {
                return Json(new { success = false, message = "We could not retrieve the post. Please contact IT" });
            }
            var user = await _mgr.FindByIdAsync(post.UserID);

            if (isapproved)
            {
                post.HasApproval = isapproved;
                post.IsApproved = isapproved;
                post.DateApproved = DateTime.UtcNow;
                _uow.Posts.Update(post);
                _uow.SaveChanges();

                //await _mgr.CustomSendEmailAsync(user.Id, "Video approval", "Your video has been approved you can now send the video to customer", user.Email, "");

                return Json(new { success = true, message = "You've successfully approved the video. He/she will receive an email notification" });
            }
            post.HasApproval = true;
            post.IsApproved = isapproved;
            _uow.Posts.Update(post);
            _uow.SaveChanges();

            //await _mgr.CustomSendEmailAsync(user.Id, "Video approval", "Your video has been disapproved.", user.Email, "");

            return Json(new { success = true, message = "You've successfully disapproved the video. He/she will receive an email notification" });            
        }

        [AllowAnonymous]
        public async Task<ActionResult> VideoHasPlayed(int postID, string userName, string details)
        {
            var post = _uow.Posts.GetById(postID);
            var user = await _mgr.FindByNameAsync(userName);

            var success = false;            

            if (post != null)
            {
                if (!post.HasPlayedVideo && post.DatePlayedVideo == null && user != null)
                {
                    post.HasPlayedVideo = true;
                    post.DatePlayedVideo = DateTime.UtcNow;
                    _uow.Posts.Update(post);
                    await _uow.SaveChangesAsync();
                    success = true;
                    //alert the SA 
                    //await _mgr.CustomSendEmailAsync(user.Id, "Your video has been viewed.", "Your video has been viewed. See the details: " + details, user.Email, user.EmailPass);
                    //await _mgr.OoredooSendSmsAsync("97470064955", $"Your video with plate number {post.PlateNumber} has been played. You can now contact the customer");
                }
            }                        
            return Json(new { success = success  });
        }        
    }
}