using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paems.Interfaces
{
    public class ImagesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string imgUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string imgTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long status { get; set; }
    }

    public class CommentsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string creationTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string isDelete { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string isTop { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string userImageUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long topped { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long replyCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long imageStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long usefulVoteCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long userClient { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long discussionId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long imageCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long anonymousFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long plusAvailable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mobileVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ImagesItem> images { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long mergeOrderStatus { get; set; }
        /// <summary>
        /// 14Ӣ��8��R7���巭ת������
        /// </summary>
        public string productColor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string productSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long textlongegral { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long imagelongegral { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string referenceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string referenceTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long replyCount2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string userImage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long orderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long longegral { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string productSales { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string referenceImage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string referenceName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long firstCategory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long secondCategory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long thirdCategory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string aesPin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long days { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long afterDays { get; set; }
    }

    public class HotCommentTagStatisticsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string canBeFiltered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long stand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string rid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ckeKeyWordBury { get; set; }
    }

    public class ProductCommentSummary
    {
        /// <summary>
        /// 
        /// </summary>
        public long skuId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long averageScore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long defaultGoodCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string defaultGoodCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long commentCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string commentCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long goodCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string goodCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double goodRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long goodRateShow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long generalCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string generalCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double generalRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long generalRateShow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long poorCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string poorCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double poorRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long poorRateShow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long videoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string videoCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long afterCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string afterCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long showCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string showCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long oneYear { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long sensitiveBook { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long fixCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long plusCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string plusCountStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long buyerShow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long poorRateStyle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long generalRateStyle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long goodRateStyle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long installRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long productId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score1Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score2Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score3Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score4Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score5Count { get; set; }
    }

    public class Response
    {
        /// <summary>
        /// 
        /// </summary>
        public string jwotestProduct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<CommentsItem> comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long soType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string csv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long imageListCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<HotCommentTagStatisticsItem> hotCommentTagStatistics { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string testId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string vTagStatistics { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long maxPage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ProductCommentSummary productCommentSummary { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string productAttr { get; set; }
    }
}
