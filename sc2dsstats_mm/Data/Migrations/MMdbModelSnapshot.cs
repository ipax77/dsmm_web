﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using dsmm_server.Data;

namespace sc2dsstats_mm.Migrations
{
    [DbContext(typeof(MMdb))]
    partial class MMdbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0-preview8.19405.11");

            modelBuilder.Entity("dsmm_server.Data.MMdbPlayer", b =>
                {
                    b.Property<int>("MMdbPlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthName")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Credential")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Ladder")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("MMDeleted")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mode2")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Server")
                        .HasColumnType("TEXT");

                    b.HasKey("MMdbPlayerId");

                    b.ToTable("MMdbPlayers");
                });

            modelBuilder.Entity("dsmm_server.Data.MMdbRace", b =>
                {
                    b.Property<int>("MMdbRaceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthName")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Credential")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Deleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Ladder")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("MMDeleted")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mode2")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Server")
                        .HasColumnType("TEXT");

                    b.HasKey("MMdbRaceId");

                    b.ToTable("MMdbRaces");
                });

            modelBuilder.Entity("dsmm_server.Data.MMdbRaceRating", b =>
                {
                    b.Property<int>("MMdbRaceRatingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("EXP")
                        .HasColumnType("REAL");

                    b.Property<int>("Games")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Lobby")
                        .HasColumnType("TEXT");

                    b.Property<int>("MMdbRaceId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("MU")
                        .HasColumnType("REAL");

                    b.Property<double>("SIGMA")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("MMdbRaceRatingId");

                    b.HasIndex("MMdbRaceId");

                    b.ToTable("MMdbRaceRatings");
                });

            modelBuilder.Entity("dsmm_server.Data.MMdbRating", b =>
                {
                    b.Property<int>("MMdbRatingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("EXP")
                        .HasColumnType("REAL");

                    b.Property<int>("Games")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Lobby")
                        .HasColumnType("TEXT");

                    b.Property<int>("MMdbPlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("MU")
                        .HasColumnType("REAL");

                    b.Property<double>("SIGMA")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("MMdbRatingId");

                    b.HasIndex("MMdbPlayerId");

                    b.ToTable("MMdbRatings");
                });

            modelBuilder.Entity("dsmm_server.Data.MMdbRaceRating", b =>
                {
                    b.HasOne("dsmm_server.Data.MMdbRace", "MMdbRace")
                        .WithMany("MMdbRaceRatings")
                        .HasForeignKey("MMdbRaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("dsmm_server.Data.MMdbRating", b =>
                {
                    b.HasOne("dsmm_server.Data.MMdbPlayer", "MMdbPlayer")
                        .WithMany("MMdbRatings")
                        .HasForeignKey("MMdbPlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
