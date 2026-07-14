using FusionOS.Modules.Core.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class UserCompanyRoleConfiguration : IEntityTypeConfiguration<UserCompanyRole>
{
    public void Configure(EntityTypeBuilder<UserCompanyRole> builder)
    {
        builder.ToTable("user_company_roles");
        builder.HasKey(x => new { x.UserId, x.CompanyId, x.RoleId });
    }
}
