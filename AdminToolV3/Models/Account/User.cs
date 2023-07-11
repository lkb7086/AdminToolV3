using System.ComponentModel.DataAnnotations;

namespace AdminToolV3.Models.Account
{
    public class User
    {
        [Required(ErrorMessage = "아이디를 입력해주세요.")]
        public string UserID { get; set; } = string.Empty;

        [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
        public string Password { get; set; } = string.Empty;
    }
}