using System.ComponentModel.DataAnnotations;

namespace GeekShopping.ProductAPI.Controllers.Auth;

public class User
{
    [Key]
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}