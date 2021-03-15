using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restfull_API_Project.Data;
using Restfull_API_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Restfull_API_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private CWheelsDbContext _cWheelsDbContext;
        public VehicleController(CWheelsDbContext cWheelsDbContext)
        {
            _cWheelsDbContext = cWheelsDbContext;
        }

        /// <summary>
        /// POST a new vehicle into the db 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] Vehicle vehicle)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            var newVehicle = new Vehicle()
            {
                Title = vehicle.Title,
                Description = vehicle.Description,
                Color = vehicle.Color,
                Company = vehicle.Company,
                Condition = vehicle.Condition,
                DatePosted = vehicle.DatePosted,
                Engine = vehicle.Engine,
                Price = vehicle.Price,
                Model = vehicle.Model,
                Location = vehicle.Location,
                CategoryId = vehicle.CategoryId,
                IsHotAndNew = false,
                IsFeatured = false,
                UserId = user.Id
            };

            _cWheelsDbContext.Vehicles.Add(newVehicle);
            _cWheelsDbContext.SaveChanges();

            return Ok(new { vehicleId = newVehicle.Id, message = "Vehicle Added Successfully" });
        }


        /// <summary>
        /// GET all the vehicles that are makred IsHotAndNew
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult HotAndNewAds()
        {
            var vehicles = from v in _cWheelsDbContext.Vehicles
                           where v.IsHotAndNew == true
                           select new
                           {
                               Id = v.Id,
                               Title = v.Title,
                               ImageUrl = v.Images.FirstOrDefault().ImageUrl
                           };

            return Ok(vehicles);
        }

        /// <summary>
        /// GETs the vehicle thats its title starts with the passed parameter
        /// </summary>
        /// <param name="search"></param>
        /// <returns>IActionResult</returns>
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult SearchVehicles(string search)
        {
            var vehicles = from v in _cWheelsDbContext.Vehicles
                           where v.Title.StartsWith(search)
                           select new
                           {
                               Id = v.Id,
                               Title = v.Title,
                           };

            return Ok(vehicles);
        }

        /// <summary>
        /// GET's a vehicle according to the passed id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>IActionResult</returns>

        [HttpGet]
        [Authorize]
        public IActionResult GetVehicle(int categoryId)
        {
            var vehicles = from v in _cWheelsDbContext.Vehicles
                           where v.CategoryId == categoryId
                           select new
                           {
                               Id = v.Id,
                               Title = v.Title,
                               Price = v.Price,
                               Location = v.Location,
                               DatePosted = v.DatePosted,
                               IsFeatured = v.IsFeatured,
                               ImageUrl = v.Images.FirstOrDefault().ImageUrl
                           };

            return Ok(vehicles);
        }

        /// <summary>
        /// GETs the vehicle details
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult VehicleDetails(int id)
        {
            var foundVehicle = _cWheelsDbContext.Vehicles.Find(id);
            if(foundVehicle == null)
            {
                return NoContent();
            }

            var vehicle = from v in _cWheelsDbContext.Vehicles
                          join u in _cWheelsDbContext.Users on v.UserId equals u.Id
                          where v.Id == id
                          select new
                          {
                              Id = v.Id,
                              Title = v.Title,
                              Description = v.Description,
                              Price = v.Price,
                              Model = v.Model,
                              Engine = v.Engine,
                              Color = v.Color,
                              Company = v.Company,
                              DatePosted = v.DatePosted,
                              Condition = v.Condition,
                              Location = v.Location,
                              Images = v.Images,
                              Email = u.Email,
                              Phone = u.Phone,
                              UserImage = u.ImageUrl
                          };

            return Ok(vehicle);


        }

        /// <summary>
        /// GETs the vehicles that the user posted
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult MyAds()
        {

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }


            var vehicle = from v in _cWheelsDbContext.Vehicles
                          where v.UserId == user.Id
                          select new
                          {
                              Id = v.Id,
                              Title = v.Title,
                              Price = v.Price,
                              Location = v.Location,
                              DatePosted = v.DatePosted,
                              IsFeatured = v.IsFeatured,
                              ImageUrl = v.Images.FirstOrDefault().ImageUrl
                          };

            return Ok(vehicle);


        }

        /// <summary>
        /// DELETEs a vehicle if it belongs to the user
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            var vehicle = _cWheelsDbContext.Vehicles.FirstOrDefault(u => u.Id == id && user.Id == u.UserId);
            
            if(vehicle == null)
            {
                return NotFound(); 
            }
            else
            {
                _cWheelsDbContext.Vehicles.Remove(vehicle);
                _cWheelsDbContext.SaveChanges();
                return Ok("Vehicle with id " + id + " was removed from your listings");
            }

        }



    }
}
