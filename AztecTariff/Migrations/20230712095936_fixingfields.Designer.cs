﻿// <auto-generated />
using AztecTariff.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AztecTariff.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20230712095936_fixingfields")]
    partial class fixingfields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("AztecTariff.Models.Pricing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("EstateId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProductId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SalesAreaId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Pricing");
                });

            modelBuilder.Entity("AztecTariff.Models.Product", b =>
                {
                    b.Property<string>("ProductId")
                        .HasColumnType("TEXT");

                    b.Property<double>("ABV")
                        .HasColumnType("REAL");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EntityCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("EstateId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Included")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Portion")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ProdName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ProductTariffName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TariffCategory")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ProductId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("AztecTariff.Models.SalesArea", b =>
                {
                    b.Property<int>("SalesAreaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EstateId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Included")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SAName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SiteId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SiteName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TariffName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("SalesAreaId");

                    b.ToTable("SalesAreas");
                });
#pragma warning restore 612, 618
        }
    }
}
