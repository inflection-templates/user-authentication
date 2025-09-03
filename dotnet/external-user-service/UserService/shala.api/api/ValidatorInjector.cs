using FluentValidation;
using shala.api.domain.types;

namespace shala.api;

public static class ValidatorInjector
{
    public static void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IValidator<ApiKeyCreateModel>, ApiKeyCreateModelValidator>();
        builder.Services.AddScoped<IValidator<ApiKeyCreateRequestModel>, ApiKeyCreateRequestModelValidator>();
        builder.Services.AddScoped<IValidator<ApiKeySearchFilters>, ApiKeySearchFiltersValidator>();

        builder.Services.AddScoped<IValidator<ClientAppCreateModel>, ClientAppCreateModelValidator>();
        builder.Services.AddScoped<IValidator<ClientAppUpdateModel>, ClientAppUpdateModelValidator>();
        builder.Services.AddScoped<IValidator<ClientAppSearchFilters>, ClientAppSearchFiltersValidator>();

        builder.Services.AddScoped<IValidator<RoleCreateModel>, RoleCreateModelValidator>();
        builder.Services.AddScoped<IValidator<RoleUpdateModel>, RoleUpdateModelValidator>();
        builder.Services.AddScoped<IValidator<RoleSearchFilters>, RoleSearchFiltersValidator>();

        builder.Services.AddScoped<IValidator<TenantCreateModel>, TenantCreateModelValidator>();
        builder.Services.AddScoped<IValidator<TenantUpdateModel>, TenantUpdateModelValidator>();
        builder.Services.AddScoped<IValidator<TenantSearchFilters>, TenantSearchFiltersValidator>();

        builder.Services.AddScoped<IValidator<UserCreateModel>, UserCreateModelValidator>();
        builder.Services.AddScoped<IValidator<UserUpdateModel>, UserUpdateModelValidator>();
        builder.Services.AddScoped<IValidator<UserSearchFilters>, UserSearchFiltersValidator>();

        builder.Services.AddScoped<IValidator<UserPasswordLoginModel>, UserPasswordLoginModelValidator>();
        builder.Services.AddScoped<IValidator<UserOtpLoginModel>, UserOtpLoginModelValidator>();
        builder.Services.AddScoped<IValidator<UserSendOtpModel>, UserSendOtpModelValidator>();
        builder.Services.AddScoped<IValidator<UserResetPasswordSendLinkModel>, UserResetPasswordSendLinkModelValidator>();
        builder.Services.AddScoped<IValidator<UserResetPasswordModel>, UserResetPasswordModelValidator>();
        builder.Services.AddScoped<IValidator<UserChangePasswordModel>, UserChangePasswordModelValidator>();
        builder.Services.AddScoped<IValidator<UserRefreshTokenModel>, UserRefreshTokenModelValidator>();
        builder.Services.AddScoped<IValidator<UserTotpValidationModel>, UserTotpValidationModelValidator>();

        builder.Services.AddScoped<IValidator<FileResourceSearchFilters>, FileResourceSearchFiltersValidator>();
        builder.Services.AddScoped<IValidator<FileResourceCreateModel>, FileResourceCreateModelValidator>();
    }
}

