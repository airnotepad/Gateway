using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult Users()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UsersTable([FromBody] DTParameters dtParameters)
        {
            var response = new APIResponse<DTResult<User>>();

            try
            {
                var searchBy = dtParameters.Search?.Value;

                var orderCriteria = string.Empty;
                var orderAscendingDirection = true;

                if (dtParameters.Order != null)
                {
                    orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                    orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
                }
                else
                {
                    orderCriteria = "Id";
                    orderAscendingDirection = true;
                }

                var result = string.IsNullOrEmpty(searchBy)
                    ? _context.Users.AsNoTracking().ToList()
                    : _context.Users.AsNoTracking().Where(x =>
                            x.Username.ToString().ToUpper().Contains(searchBy.ToUpper())
                            )
                        .ToList();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<User>
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = result.Count,
                    data = result
                        .Skip(dtParameters.Start)
                        .Take(dtParameters.Length)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        [HttpPost]
        public JsonResult SaveUser([FromBody] User data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                var user = _context.Users.FirstOrDefault(x => x.Id == data.Id);
                if (user == null) throw new Exception($"User with such id not found");

                user.Percent = data.Percent;
                user.Role = data.Role;

                _context.SaveChanges();

                result.Result = true;
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult InvitesTable([FromBody] DTParameters dtParameters)
        {
            var response = new APIResponse<DTResult<Invite>>();

            try
            {
                var searchBy = dtParameters.Search?.Value;

                var orderCriteria = string.Empty;
                var orderAscendingDirection = true;

                if (dtParameters.Order != null)
                {
                    orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                    orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
                }
                else
                {
                    orderCriteria = "Text";
                    orderAscendingDirection = true;
                }

                var result = _inviteService.GetAll();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<Invite>
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        [HttpPost]
        public JsonResult DeleteInvite([FromBody] Invite data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                result.Result = _inviteService.Delete(data.Text); ;
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult AddInvite([FromBody] Invite data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                if (string.IsNullOrEmpty(data?.Text)) throw new Exception("Text is null or empty");
                result.Result = _inviteService.AddNew(data.Text); ;
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }
    }
}
