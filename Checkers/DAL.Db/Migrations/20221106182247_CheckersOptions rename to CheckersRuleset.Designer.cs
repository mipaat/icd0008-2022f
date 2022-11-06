﻿// <auto-generated />
using System;
using DAL.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DAL.Db.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20221106182247_CheckersOptions rename to CheckersRuleset")]
    partial class CheckersOptionsrenametoCheckersRuleset
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.10");

            modelBuilder.Entity("Domain.CheckersGame", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BlackPlayerId")
                        .HasColumnType("TEXT");

                    b.Property<int>("CheckersOptionsId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("CheckersRulesetId");

                    b.Property<DateTime?>("EndedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("WhitePlayerId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Winner")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CheckersOptionsId");

                    b.ToTable("CheckersGames");
                });

            modelBuilder.Entity("Domain.CheckersOptions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BlackMovesFirst")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BuiltIn")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CanJumpBackwards")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CanJumpBackwardsDuringMultiCapture")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("TEXT");

                    b.Property<bool>("MustCapture")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Saved")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("CheckersRulesets");
                });

            modelBuilder.Entity("Domain.CheckersState", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BlackMoves")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CheckersGameId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("GameElapsedTime")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("MoveElapsedTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SerializedGamePieces")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("WhiteMoves")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CheckersGameId");

                    b.ToTable("CheckersStates");
                });

            modelBuilder.Entity("Domain.CheckersGame", b =>
                {
                    b.HasOne("Domain.CheckersOptions", "CheckersOptions")
                        .WithMany()
                        .HasForeignKey("CheckersOptionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CheckersOptions");
                });

            modelBuilder.Entity("Domain.CheckersState", b =>
                {
                    b.HasOne("Domain.CheckersGame", null)
                        .WithMany("CheckersStates")
                        .HasForeignKey("CheckersGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.CheckersGame", b =>
                {
                    b.Navigation("CheckersStates");
                });
#pragma warning restore 612, 618
        }
    }
}