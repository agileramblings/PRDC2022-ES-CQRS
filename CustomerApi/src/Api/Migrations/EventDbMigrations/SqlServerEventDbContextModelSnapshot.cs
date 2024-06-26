﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PRDC2022.CustomerApi.Persistence;

#nullable disable

namespace PRDC2022.CustomerApi.Migrations.EventDbMigrations
{
    [DbContext(typeof(SqlServerEventDbContext))]
    partial class SqlServerEventDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("PRDC2022.CustomerApi.Persistence.EventDescriptorEntity", b =>
                {
                    b.Property<string>("AggregateId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AggregateType")
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.Property<Guid>("CausationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ReceivedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("AggregateId", "AggregateType", "Version")
                        .HasName("PKC_EventDescriptors_AggregateId");

                    b.ToTable("EventDescriptors");
                });

            modelBuilder.Entity("PRDC2022.CustomerApi.Persistence.SnapshotEntity", b =>
                {
                    b.Property<string>("AggregateId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AggregateType")
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.Property<Guid>("CausationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ReceivedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("AggregateId", "AggregateType", "Version")
                        .HasName("PKC_Snapshots_AggregateId");

                    b.ToTable("Snapshots");
                });
#pragma warning restore 612, 618
        }
    }
}
