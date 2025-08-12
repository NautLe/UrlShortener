using FluentValidation;
using UrlShortener.Models;

namespace UrlShortener.Validators
{
    public class ShortenRequestValidator : AbstractValidator<ShortenRequest>
    {
        public ShortenRequestValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("URL không được để trống.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("URL không hợp lệ.")
                .Must(url => !(url?.Contains("localhost") ?? false))
                    .WithMessage("Không được rút gọn URL localhost.")
                .Must(url => !(url?.Contains("127.0.0.1") ?? false))
                    .WithMessage("Không được rút gọn URL localhost (127.0.0.1).");
        }
    }
}
