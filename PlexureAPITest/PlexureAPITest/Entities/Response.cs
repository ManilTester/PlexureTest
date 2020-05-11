using System.Net;
using NUnit.Framework;

namespace PlexureAPITest.Entities
{
    public class Response<T> where T : IEntity
    {
        public Response(HttpStatusCode statusCode, string error)
        {
            StatusCode = statusCode;
            Error = error;
        }

        public Response(HttpStatusCode statusCode, T entity)
        {
            StatusCode = statusCode;
            Entity = entity;
        }

        public string Error { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public T Entity { get; set; }

        // MARK - TestCase convenience

        /// <summary>
        /// Assertion for different API Calls
        /// </summary>
        /// <param name="statusCode"> Status code returned from the API</param>
        /// <param name="errorMessage"> Error messages to be displayed if API fails</param>
        public void Expect(HttpStatusCode statusCode,string errorMessage)
        {
            Assert.AreEqual(StatusCode, statusCode,errorMessage);
        }
    }
}
