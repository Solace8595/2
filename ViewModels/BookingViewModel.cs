using System.ComponentModel.DataAnnotations;

namespace Pastar.ViewModels
{
    public class BookingViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Введите номер телефона")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Выберите способ связи")]
        public long ConnectionMethodId { get; set; }

        [Required(ErrorMessage = "Выберите дату и время")]
        public DateTime BookingDateTime { get; set; }

        [Range(1, 20, ErrorMessage = "Укажите количество человек от 1 до 20")]
        public int NumberOfPeople { get; set; }
    }
}
