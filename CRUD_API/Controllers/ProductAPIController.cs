using AutoMapper;
using Company_API.Data;
using Company_API.Models;
using Company_API.Models.DTO;
using Company_API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Company_API.Controllers

{
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    [Route("api/ProductAPI")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IProductRepository _dbProduct;
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public ProductAPIController(IProductRepository dbProduct, IMapper mapper, DatabaseContext dbContext)
        {
            _dbProduct = dbProduct;
            _mapper = mapper;
            _response = new();
            _dbContext = dbContext;
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = "Default30")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<ActionResult<APIResponse>> GetProducts(
            [FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
        {
            try
            {

                IEnumerable<Product> productList;
            
                productList = await _dbProduct.GetAllAsync(pageSize: pageSize,
                        pageNumber: pageNumber);
                
                if (!string.IsNullOrEmpty(search))
                {
                    productList = productList.Where(u => u.Name.ToLower().Contains(search));
                }
                Pagination pagination = new() { PageNumber = pageNumber, PageSize = pageSize }; 

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
                _response.Result = _mapper.Map<List<ProductDTO>>(productList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [HttpGet("{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetProduct(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var product = await _dbProduct.GetAsync(u => u.Id == id);
                if (product == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<ProductDTO>(product);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> CreateProduct([FromBody] ProductCreateDTO createDTO)
        {
            try
            {
    
                if (await _dbProduct.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Product already Exists!");
                    return BadRequest(ModelState);
                }

                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }
                
                Product product = _mapper.Map<Product>(createDTO);

                string userName = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == userName);


                // Get the CompanyId of the company for which the product is being created
                int companyId = createDTO.CompanyId;

                // Query the CompanyPermission table to check if there is a record with the matching UserId and CompanyId
                var companyPermission = await _dbContext.CompanyPermissions.FirstOrDefaultAsync(cp => cp.UserId == user.Id && cp.CompanyId == companyId);

                // If there is no matching record in the CompanyPermission table, return a Forbidden response
                if (companyPermission == null)
                {
                    return Forbid();
                }

                await _dbProduct.CreateAsync(product);
                _response.Result = _mapper.Map<ProductDTO>(product);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetProduct", new { id = product.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        public async Task<ActionResult<APIResponse>> DeleteProduct(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbProduct.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }
                await _dbProduct.RemoveAsync(villa);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

        public async Task<ActionResult<APIResponse>> UpdateProduct(int id, [FromBody] ProductUpdateDTO updateDTO) 
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    return BadRequest();
                }

                Product model = _mapper.Map<Product>(updateDTO);

                await _dbProduct.UpdateAsync(model);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.ToString() }; 
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialProduct")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

        public async Task<IActionResult> UpdatePartialProduct(int id, JsonPatchDocument<ProductUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var product = await _dbProduct.GetAsync(u => u.Id == id, tracked: false);

            ProductUpdateDTO productDTO = _mapper.Map<ProductUpdateDTO>(product);


            if (product == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(productDTO, ModelState);
            Product model = _mapper.Map<Product>(productDTO);

            await _dbProduct.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        } 
    } 
}