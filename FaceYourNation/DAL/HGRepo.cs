﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FaceYourNation.Models;
using System.Data.Entity;
using System.Text;

namespace FaceYourNation.DAL
{
    public class HGRepo
    {
        public HGContext context { get; set; }

        public HGRepo()
        {
            context = new HGContext();
        }

        public HGRepo(HGContext _context)
        {
            context = _context;
        }

        //Private Methods to DRY code

        private void S()
        {
            context.SaveChanges();
        }

        private string san(string str) //sanitize the data
        {
            str = str.ToLower();
            if (str.IndexOf('.') > -1)
            {
                for (int i = 0; i < str.Length; i++) //using a for loop in case of multiple dots.
                {
                    if (str[i] == '.') //remember to sanitize incoming data for things other than dots on frontend.
                    {
                        str = str.Remove(i, 1);
                    }
                }
            }
            return str;
        }

        private Issue GetIssueByName(string iss_name)
        {
            iss_name = san(iss_name);
            var Issues = context.Issues;
            var _issue = Issues.FirstOrDefault(i => i.Name.ToLower() == iss_name);
            return _issue;
        }

        private Bill GetBillById(string id)
        {
            id = san(id);
            var Bills = context.Bills;
            var _bill = Bills.FirstOrDefault(b => (b.HouseID == id) || (b.SenateID == id));
            return _bill;
        }

        private Vote SetVote(string dis, string vid, bool _bool, int import = 5)
        {
            dis = san(dis);
            Vote vote = new Vote();
            vote.District = dis;
            vote.video_id = vid;
            vote.LogPublicSupport(_bool);
            vote.importance = import;
            context.Votes.Add(vote);
            return vote;
        }

        //Public Methods for Getting/Manipulating Data
        
        public List<Bill> GetBills()
        {
            return context.Bills.ToList();
        }

        public List<Issue> GetIssues()
        {
            return context.Issues.ToList();
        }

        public Issue GetIssue(string iss_name)
        {
            return GetIssueByName(iss_name);
        }

        public Bill GetBill(string id)
        {
            return GetBillById(id);
        }

        public void AddIssuePosition(string iss_name, string dis, string vid, bool _bool, int import = 5)
        {
            iss_name = san(iss_name);
            dis = san(dis);
            Issue _iss = GetIssueByName(iss_name);
            Vote vote = SetVote(dis, vid, _bool, import);
            vote.issue_name = iss_name;
            _iss.AddVote(vote);
            S();
        }

        public PositionResult GetIssuePublicPosition(string iss_name, string dis = "")
        {
            iss_name = san(iss_name);
            Issue iss = GetIssueByName(iss_name);
            Random r = new Random();
            PositionResult result = new PositionResult();

            IQueryable<Vote> q = context.Votes.AsQueryable();
            q = q.Where(v => v.issue_name == iss_name);
            if (dis != "")
            {
                dis = san(dis);
                q = q.Where(v => v.District == dis);
                result.District = dis;
            }

            List<Vote> votes = q.ToList();
            double AvgImport = votes.Average(v => v.importance);
            int For = votes.OrderBy(v => (v.support == "For")).Count();
            int Against = votes.OrderBy(v => (v.support == "Against")).Count();

            
            result.Issue_Name = iss.Name;
            result.For = For;
            result.Against = Against;
            result.AvgImportance = Math.Round(AvgImport, 2);
            int j = Convert.ToInt32(r.Next(0, votes.Count));
            result.VideoId = votes[j].video_id;
            return result;
        }

        public string GetIssuePresidentialPosition(string iss_name)
        {
            iss_name = san(iss_name);
            Issue iss = GetIssueByName(iss_name);
            return iss.PresidentialPosition;
        }

        public string GetIssuePresidentialURL(string iss_name)
        {
            iss_name = san(iss_name);
            Issue iss = GetIssueByName(iss_name);
            return iss.PresPositionURL;
        }

        public void SetIssuePresidentialPosition(string iss_name, string url)
        {
            iss_name = san(iss_name);
            Issue iss = GetIssueByName(iss_name);
            iss.AddPresidentialPositionURL(url);
            S();
        }

        public void AddBill(string name, string the_bill, string house = "", string senate = "", bool pres_support = false)
        {
            house = san(house);
            senate = san(senate);
            Bill bill = new Bill();
            bill.Name = name;
            bill.theBill = the_bill;
            if (house != "")
            {
                if (GetBillById(house) != null)
                {
                    bill = null;
                    return;
                }
                bill.HouseID = house;
            }
            if (senate != "")
            {
                if (GetBillById(senate) != null)
                {
                    bill = null;
                    return;
                }
                bill.SenateID = senate;
            }
            bill.PresidentialSupport = pres_support;
            context.Bills.Add(bill);
            S();
        }

        public PositionResult GetBillPublicPosition(string house = "", string senate = "", string dis = "")
        {
            PositionResult result = new PositionResult();
            Random r = new Random();
            IQueryable<Vote> q = context.Votes.AsQueryable<Vote>();
            if (house != "")
            {
                house = san(house);
                q = q.Where(v => v.house_id == house);
            }
            else if (senate != "")
            {
                senate = san(senate);
                q = q.Where(v => v.senate_id == senate);
            }
            else
            {
                throw new ArgumentException("Resolution not found. Please enter a valid House or Senate Resolution number. Ex: hr1420");
            }

            if (dis != "")
            {
                dis = san(dis);
                q = q.Where(v => v.District == dis);
                result.District = dis;
            }
            List<Vote> votes = q.ToList();
            double AvgImport = votes.Average(v => v.importance);
            int For = votes.OrderBy(v => (v.support == "For")).Count();
            int Against = votes.OrderBy(v => (v.support == "Against")).Count();
            int j = Convert.ToInt32(r.Next(0, votes.Count));

            result.For = For;
            result.Against = Against;
            result.AvgImportance = Math.Round(AvgImport, 2);
            result.VideoId = votes[j].video_id;

            return result;
        }

        public void AddBillPosition(string vid, string dis, bool _bool, string house = "", string senate = "", int import = 5)
        {
            Bill bill = null;
            dis = san(dis);
            Vote vote = SetVote(dis, vid, _bool, import);
            if (house != "")
            {
                house = san(house);
                bill = GetBillById(house);
                vote.house_id = house;
            } else if (senate != "")
            {
                senate = san(senate);
                bill = GetBillById(senate);
                vote.senate_id = senate;
            }
            bill.AddVote(vote);
            S();
        }
    }
}