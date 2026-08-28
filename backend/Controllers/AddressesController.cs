using EcSite.Api.Data;
using EcSite.Api.DTOs.Orders;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AddressesController(AppDbContext db) => _db = db;

    private static AddressDto ToDto(Address a) => new(a.Id, a.Recipient, a.Phone, a.Province, a.City, a.Detail, a.IsDefault);

    [HttpGet]
    public async Task<ActionResult<List<AddressDto>>> Get()
    {
        var userId = User.GetUserId();
        var list = await _db.Addresses.AsNoTracking().Where(a => a.UserId == userId).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(UpsertAddressRequest request)
    {
        var userId = User.GetUserId();
        if (request.IsDefault)
        {
            var others = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            others.ForEach(a => a.IsDefault = false);
        }

        var address = new Address
        {
            UserId = userId, Recipient = request.Recipient, Phone = request.Phone,
            Province = request.Province, City = request.City, Detail = request.Detail,
            IsDefault = request.IsDefault
        };
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();
        return Ok(ToDto(address));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AddressDto>> Update(int id, UpsertAddressRequest request)
    {
        var userId = User.GetUserId();
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address is null) return NotFound();

        if (request.IsDefault)
        {
            var others = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault && a.Id != id).ToListAsync();
            others.ForEach(a => a.IsDefault = false);
        }

        address.Recipient = request.Recipient;
        address.Phone = request.Phone;
        address.Province = request.Province;
        address.City = request.City;
        address.Detail = request.Detail;
        address.IsDefault = request.IsDefault;
        await _db.SaveChangesAsync();
        return Ok(ToDto(address));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address is null) return NotFound();

        var usedByOrder = await _db.Orders.AnyAsync(o => o.AddressId == id);
        if (usedByOrder) return BadRequest(new { message = "この住所は過去の注文で使用されているため削除できません" });

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
