using System.Collections.Generic;
using System.Configuration;
using System.Net;
using NUnit.Framework;
using PlexureAPITest.Services;

namespace PlexureAPITest
{
    [TestFixture]
    public class Test
    {
        private Service _service;

        // Injecting context to share values between different tests
        private readonly Context.Context _context;

        private static Dictionary<string, string> GetCredentials()
        {
            string username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["password"];

            var credentials = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            return credentials;
        }

        public Test()
        {
            _context = new Context.Context();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            _service = new Service();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }

        [Test]
        public void TEST_001_Login_With_Valid_User()
        {
            //Arrange
            var credentials = GetCredentials();

            //Act
            var response = _service.Login(credentials["username"], credentials["password"]);

            //Assert
            response.Expect(HttpStatusCode.OK, "Failed to Login");
            Assert.AreEqual(1, response.Entity.UserId, "UserId for logged in user incorrect");
            Assert.AreEqual(credentials["username"], response.Entity.UserName, "UserName for logged in user incorrect");
        }


        [Test]
        public void TEST_002_Get_Points_For_Logged_In_User()
        {
            //Act
            var points = _service.GetPoints();

            //Assert
            points.Expect(HttpStatusCode.Accepted, "Failed to get points");

            // Set value for the context variable
            _context.PointsBeforePurchase = points.Entity.Value;
        }

        [Test]
        public void TEST_003_Purchase_Product()
        {
            //Arrange
            int productId = 1;

            //Act
            var purchaseResponse = _service.Purchase(productId);

            //Assert
            purchaseResponse.Expect(HttpStatusCode.Accepted,
                "Fail to purchase the product with id : " + productId);
        }

        [Test]
        public void TEST_004_Verify_On_Product_Purchase_TotalPoints_Increase_By_100()
        {
            //Act
            var response = _service.GetPoints();
            var pointsAfterPurchase = response.Entity.Value;

            var increaseInPointsAfterPurchase = pointsAfterPurchase - _context.PointsBeforePurchase;

            //Assert

            Assert.IsTrue(increaseInPointsAfterPurchase == 100, "After purchase points didn't increase by 100");
        }

        [Test]
        public void TEST_005_Negative_Login_With_Empty_User_Credentials()
        {
            //Act
            var response = _service.Login("", "");

            var errorMessage = response.Error.Replace('"', ' ');

            //Assert
            Assert.AreEqual(" Error: Username and password required. ", errorMessage);
        }

        [Test]
        [TestCase("Testar", "Plexure123")]
        [TestCase("Tester", "Plexure12")]
        [TestCase("", "Plexure123")]
        [TestCase("Tester", "")]
        [TestCase("Tester", "$&^$")]
        public void TEST_006_Negative_Login_With_Invalid_User_Credentials(string user, string password)
        {
            //Act
            var response = _service.Login(user, password);

            var errorMessage = response.Error.Replace('"', ' ');

            //Assert
            Assert.AreEqual(" Error: Unauthorized ", errorMessage);
        }


        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(999999999)]
        [TestCase(-999999999)]
        public void TEST_007_Negative_Purchase_Product_With_Invalid_Product_Id(int productId)
        {
            //Act
            var purchaseResponse = _service.Purchase(productId);

            //Assert
            purchaseResponse.Expect(HttpStatusCode.BadRequest,
                "Test failed for productId : " + productId);
            var errorMessage = purchaseResponse.Error.Replace('"', ' ');

            Assert.AreEqual(" Error: Invalid product id ", errorMessage);
        }

        [Test]
        public void TEST_008_Negative_Get_Points_For_Invalid_User()
        {
            //Arrange
            _service.Login("incorrect", "incorrect",true);

            //Act
            var pointsResponse = _service.GetPoints();

            //Assert
            pointsResponse.Expect(HttpStatusCode.Unauthorized, "Failed to get error message");
            Assert.AreEqual(" Error: Unauthorized ", pointsResponse.Error.Replace('"', ' '));
        }
    }
}