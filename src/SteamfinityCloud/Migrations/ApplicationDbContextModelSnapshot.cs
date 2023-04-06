﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;
using Steamfinity.Cloud;

#nullable disable

namespace Steamfinity.Cloud.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            OracleModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("RAW(16)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("RAW(16)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("RAW(16)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("RAW(16)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("RAW(16)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("RAW(16)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Value")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("RAW(16)");

                    b.Property<int>("Color")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTimeOffset>("CreationTime")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("Description")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("LaunchParameters")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("RAW(16)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountShare", b =>
                {
                    b.Property<Guid>("AccountId")
                        .HasColumnType("RAW(16)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("RAW(16)");

                    b.Property<bool>("IsAllowedToEdit")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsAllowedToSignIn")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsAllowedToViewPassword")
                        .HasColumnType("NUMBER(1)");

                    b.Property<DateTimeOffset>("TimeShared")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.HasKey("AccountId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("AccountShares");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountTag", b =>
                {
                    b.Property<Guid>("AccountId")
                        .HasColumnType("RAW(16)");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR2(450)");

                    b.HasKey("AccountId", "Name");

                    b.HasIndex("Name");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("RAW(16)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR2(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR2(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("\"NormalizedName\" IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("RAW(16)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR2(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsSuspended")
                        .HasColumnType("NUMBER(1)");

                    b.Property<DateTimeOffset?>("LastSignInTime")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("NUMBER(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR2(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR2(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<DateTimeOffset>("SignUpTime")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR2(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("\"NormalizedUserName\" IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.GroupMembership", b =>
                {
                    b.Property<Guid>("GroupId")
                        .HasColumnType("RAW(16)");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("RAW(16)");

                    b.HasKey("GroupId", "AccountId");

                    b.HasIndex("AccountId");

                    b.ToTable("Memberships");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.GroupShare", b =>
                {
                    b.Property<Guid>("GroupId")
                        .HasColumnType("RAW(16)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("RAW(16)");

                    b.Property<bool>("IsAllowedToEdit")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsAllowedToSignIn")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsAllowedToViewPasswords")
                        .HasColumnType("NUMBER(1)");

                    b.Property<DateTimeOffset>("TimeShared")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.HasKey("GroupId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("GroupShares");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.SteamAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("RAW(16)");

                    b.Property<string>("AccountName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Alias")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("Color")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal?>("CurrentGameId")
                        .HasColumnType("NUMBER(20)");

                    b.Property<string>("CurrentGameName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<bool?>("IsCommentingAllowed")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool?>("IsCommunityBanned")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool?>("IsProfileSetUp")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool?>("IsProfileVisible")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("LaunchParameters")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Notes")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int?>("NumberOfDaysSinceLastBan")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int?>("NumberOfGameBans")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int?>("NumberOfVACBans")
                        .HasColumnType("NUMBER(10)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("RAW(16)");

                    b.Property<string>("Password")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("ProfileName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("ProfileUrl")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("RealName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int?>("Status")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("SteamId")
                        .HasColumnType("NUMBER(20)");

                    b.Property<DateTimeOffset>("TimeAdded")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<DateTimeOffset?>("TimeCreated")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<DateTimeOffset?>("TimeEdited")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<DateTimeOffset?>("TimeSignedOut")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<DateTimeOffset?>("TimeUpdated")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("SteamId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountGroup", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", "Owner")
                        .WithMany("OwnedGroups")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountShare", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.SteamAccount", "Account")
                        .WithMany("Shares")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", "User")
                        .WithMany("AccountShares")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountTag", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.SteamAccount", "Account")
                        .WithMany("Tags")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.GroupMembership", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.SteamAccount", "Account")
                        .WithMany("Memberships")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Steamfinity.Cloud.Entities.AccountGroup", "Group")
                        .WithMany("Memberships")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.GroupShare", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.AccountGroup", "Group")
                        .WithMany("Shares")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", "User")
                        .WithMany("GroupShares")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.SteamAccount", b =>
                {
                    b.HasOne("Steamfinity.Cloud.Entities.ApplicationUser", "Owner")
                        .WithMany("OwnedAccounts")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.AccountGroup", b =>
                {
                    b.Navigation("Memberships");

                    b.Navigation("Shares");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.ApplicationUser", b =>
                {
                    b.Navigation("AccountShares");

                    b.Navigation("GroupShares");

                    b.Navigation("OwnedAccounts");

                    b.Navigation("OwnedGroups");
                });

            modelBuilder.Entity("Steamfinity.Cloud.Entities.SteamAccount", b =>
                {
                    b.Navigation("Memberships");

                    b.Navigation("Shares");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
