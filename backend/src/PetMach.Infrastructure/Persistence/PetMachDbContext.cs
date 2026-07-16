using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetMach.Domain.Adoption;
using PetMach.Domain.Chat;
using PetMach.Domain.Discovery;
using PetMach.Domain.Dogs;
using PetMach.Domain.Health;
using PetMach.Domain.Identity;
using PetMach.Domain.Matches;
using PetMach.Domain.Meetings;
using PetMach.Domain.Moderation;
using PetMach.Domain.Notifications;
using PetMach.Domain.Partners;
using PetMach.Domain.Reservations;
using PetMach.Domain.Tutors;
using PetMach.Infrastructure.Identity;

namespace PetMach.Infrastructure.Persistence;

public sealed class PetMachDbContext(DbContextOptions<PetMachDbContext> options)
    : IdentityDbContext<PetMachUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();
    public DbSet<IdentityAuditLog> IdentityAuditLogs => Set<IdentityAuditLog>();
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();
    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<DogPhoto> DogPhotos => Set<DogPhoto>();
    public DbSet<DogVaccination> DogVaccinations => Set<DogVaccination>();
    public DbSet<DewormingRecord> DewormingRecords => Set<DewormingRecord>();
    public DbSet<DogPreference> DogPreferences => Set<DogPreference>();
    public DbSet<DogLike> DogLikes => Set<DogLike>();
    public DbSet<DogPass> DogPasses => Set<DogPass>();
    public DbSet<DogMatch> DogMatches => Set<DogMatch>();
    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ConversationReadState> ConversationReadStates => Set<ConversationReadState>();
    public DbSet<DogMeeting> DogMeetings => Set<DogMeeting>();
    public DbSet<PartnerEstablishment> PartnerEstablishments => Set<PartnerEstablishment>();
    public DbSet<PartnerSpace> PartnerSpaces => Set<PartnerSpace>();
    public DbSet<SpaceAvailability> SpaceAvailabilities => Set<SpaceAvailability>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationHistoryEntry> ReservationHistory => Set<ReservationHistoryEntry>();
    public DbSet<AdoptionProfile> AdoptionProfiles => Set<AdoptionProfile>();
    public DbSet<AdoptionApplication> AdoptionApplications => Set<AdoptionApplication>();
    public DbSet<AdoptionApplicationHistory> AdoptionApplicationHistory => Set<AdoptionApplicationHistory>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportEvidence> ReportEvidence => Set<ReportEvidence>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");
        builder.Entity<PetMachUser>(entity =>
        {
            entity.Property(user => user.CreatedAtUtc).IsRequired();
            entity.Property(user => user.BirthDate).IsRequired();
            entity.Property(user => user.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(user => user.NormalizedEmail).IsUnique();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(token => token.UsedAtUtc).IsConcurrencyToken();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.FamilyId });
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConsentRecord>(entity =>
        {
            entity.ToTable("ConsentRecords");
            entity.HasKey(consent => consent.Id);
            entity.Property(consent => consent.TermsVersion).HasMaxLength(32).IsRequired();
            entity.Property(consent => consent.PrivacyVersion).HasMaxLength(32).IsRequired();
            entity.HasIndex(consent => new { consent.UserId, consent.AcceptedAtUtc });
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(consent => consent.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<IdentityAuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Action).HasMaxLength(100).IsRequired();
            entity.Property(log => log.Result).HasMaxLength(32).IsRequired();
            entity.HasIndex(log => new { log.TargetUserId, log.OccurredAtUtc });
        });

        builder.Entity<TutorProfile>(entity =>
        {
            entity.ToTable("TutorProfiles", "tutors");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(profile => profile.LastName).HasMaxLength(100).IsRequired();
            entity.Property(profile => profile.Phone).HasMaxLength(30);
            entity.Property(profile => profile.City).HasMaxLength(120).IsRequired();
            entity.Property(profile => profile.State).HasMaxLength(50).IsRequired();
            entity.Property(profile => profile.Biography).HasMaxLength(1000);
            entity.HasIndex(profile => profile.UserId).IsUnique();
            entity.HasOne<PetMachUser>().WithOne().HasForeignKey<TutorProfile>(profile => profile.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Dog>(entity =>
        {
            entity.ToTable("Dogs", "dogs"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired(); entity.Property(x => x.Breed).HasMaxLength(120).IsRequired();
            entity.Property(x => x.WeightKg).HasPrecision(5, 2); entity.Property(x => x.Temperament).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Restrictions).HasMaxLength(1000); entity.Property(x => x.SpecialNeeds).HasMaxLength(1000); entity.Property(x => x.Biography).HasMaxLength(2000);
            entity.Property(x => x.Sex).HasConversion<string>().HasMaxLength(16); entity.Property(x => x.Size).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.EnergyLevel).HasConversion<string>().HasMaxLength(16); entity.Property(x => x.Goal).HasConversion<string>().HasMaxLength(24); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.HasIndex(x => new { x.OwnerUserId, x.Status }); entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<DogPhoto>(entity =>
        {
            entity.ToTable("DogPhotos", "dogs"); entity.HasKey(x => x.Id); entity.Property(x => x.StorageKey).HasMaxLength(300).IsRequired(); entity.Property(x => x.ContentType).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.DogId, x.IsPrimary }); entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.DogId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<DogVaccination>(entity => { entity.ToTable("DogVaccinations", "health"); entity.HasKey(x => x.Id); entity.Property(x => x.VaccineName).HasMaxLength(150).IsRequired(); entity.HasIndex(x => new { x.DogId, x.AppliedOn }); entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.DogId).OnDelete(DeleteBehavior.Cascade); });
        builder.Entity<DewormingRecord>(entity => { entity.ToTable("DewormingRecords", "health"); entity.HasKey(x => x.Id); entity.Property(x => x.ProductName).HasMaxLength(150).IsRequired(); entity.HasIndex(x => new { x.DogId, x.AppliedOn }); entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.DogId).OnDelete(DeleteBehavior.Cascade); });

        builder.Entity<DogPreference>(entity =>
        {
            entity.ToTable("DogPreferences", "discovery");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DogId).IsUnique();
            entity.HasOne<Dog>().WithOne().HasForeignKey<DogPreference>(x => x.DogId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<DogLike>(entity =>
        {
            entity.ToTable("DogLikes", "discovery", table => table.HasCheckConstraint("CK_DogLikes_NoSelfLike", "\"SourceDogId\" <> \"TargetDogId\""));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SourceDogId, x.TargetDogId }).IsUnique();
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.SourceDogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.TargetDogId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<DogPass>(entity =>
        {
            entity.ToTable("DogPasses", "discovery", table => table.HasCheckConstraint("CK_DogPasses_NoSelfPass", "\"SourceDogId\" <> \"TargetDogId\""));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SourceDogId, x.TargetDogId }).IsUnique();
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.SourceDogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.TargetDogId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<DogMatch>(entity =>
        {
            entity.ToTable("DogMatches", "matches", table => table.HasCheckConstraint("CK_DogMatches_DistinctDogs", "\"DogAId\" <> \"DogBId\""));
            entity.HasKey(x => x.Id);
            entity.Ignore(x => x.IsActive);
            entity.HasIndex(x => new { x.DogAId, x.DogBId }).IsUnique();
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.DogAId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.DogBId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<BlockedUser>(entity =>
        {
            entity.ToTable("BlockedUsers", "moderation", table => table.HasCheckConstraint("CK_BlockedUsers_NoSelfBlock", "\"UserId\" <> \"BlockedUserId\""));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.BlockedUserId }).IsUnique();
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.BlockedUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications", "notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc });
            entity.HasIndex(x => new { x.RecipientUserId, x.Type, x.MatchId }).IsUnique();
            entity.HasIndex(x => new { x.RecipientUserId, x.Type, x.MeetingId }).IsUnique();
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<DogMatch>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<DogMeeting>().WithMany().HasForeignKey(x => x.MeetingId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations", "chat");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MatchId).IsUnique();
            entity.HasOne<DogMatch>().WithOne().HasForeignKey<Conversation>(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("Messages", "chat");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => new { x.ConversationId, x.SentAtUtc });
            entity.HasOne<Conversation>().WithMany().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ConversationReadState>(entity =>
        {
            entity.ToTable("ConversationReadStates", "chat");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ConversationId, x.UserId }).IsUnique();
            entity.HasOne<Conversation>().WithMany().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ChatMessage>().WithMany().HasForeignKey(x => x.LastReadMessageId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<DogMeeting>(entity =>
        {
            entity.ToTable("DogMeetings", "meetings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlaceName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => new { x.MatchId, x.CreatedAtUtc });
            entity.HasOne<DogMatch>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.ProposedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<PartnerEstablishment>(entity =>
        {
            entity.ToTable("Establishments", "partners");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LegalName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RegistrationNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.City).HasMaxLength(120).IsRequired();
            entity.Property(x => x.State).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.OwnerUserId).IsUnique();
            entity.HasIndex(x => x.RegistrationNumber).IsUnique();
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<PartnerSpace>(entity =>
        {
            entity.ToTable("Spaces", "partners");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.InformationalPrice).HasPrecision(10, 2);
            entity.HasIndex(x => new { x.EstablishmentId, x.IsActive });
            entity.HasOne<PartnerEstablishment>().WithMany().HasForeignKey(x => x.EstablishmentId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<SpaceAvailability>(entity =>
        {
            entity.ToTable("SpaceAvailabilities", "partners", table => table.HasCheckConstraint("CK_SpaceAvailabilities_Chronological", "\"StartsAtUtc\" < \"EndsAtUtc\""));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SpaceId, x.StartsAtUtc, x.EndsAtUtc }).IsUnique();
            entity.HasOne<PartnerSpace>().WithMany().HasForeignKey(x => x.SpaceId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations", "reservations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => x.AvailabilityId).IsUnique().HasFilter("\"Status\" IN ('Pending', 'Confirmed')");
            entity.HasIndex(x => new { x.RequesterUserId, x.CreatedAtUtc });
            entity.HasOne<SpaceAvailability>().WithMany().HasForeignKey(x => x.AvailabilityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Dog>().WithMany().HasForeignKey(x => x.DogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.RequesterUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ReservationHistoryEntry>(entity =>
        {
            entity.ToTable("ReservationHistory", "reservations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => new { x.ReservationId, x.OccurredAtUtc });
            entity.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AdoptionProfile>(entity =>
        {
            entity.ToTable("Profiles", "adoption");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Story).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Requirements).HasMaxLength(1500).IsRequired();
            entity.Property(x => x.TermsVersion).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => x.DogId).IsUnique();
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            entity.HasOne<Dog>().WithOne().HasForeignKey<AdoptionProfile>(x => x.DogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.PublisherUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AdoptionApplication>(entity =>
        {
            entity.ToTable("Applications", "adoption");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Motivation).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Experience).HasMaxLength(1500).IsRequired();
            entity.Property(x => x.HousingContext).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.TermsVersion).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => new { x.ProfileId, x.ApplicantUserId }).IsUnique();
            entity.HasIndex(x => x.ProfileId).IsUnique().HasFilter("\"Status\" = 'Approved'");
            entity.HasIndex(x => new { x.ApplicantUserId, x.CreatedAtUtc });
            entity.HasOne<AdoptionProfile>().WithMany().HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.ApplicantUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AdoptionApplicationHistory>(entity =>
        {
            entity.ToTable("ApplicationHistory", "adoption");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => new { x.ApplicationId, x.OccurredAtUtc });
            entity.HasOne<AdoptionApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Report>(entity =>
        {
            entity.ToTable("Reports", "moderation");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TargetType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => new { x.ReporterUserId, x.TargetType, x.TargetId }).IsUnique().HasFilter("\"Status\" IN ('Submitted', 'UnderReview')");
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.ReporterUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PetMachUser>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ReportEvidence>(entity =>
        {
            entity.ToTable("ReportEvidence", "moderation");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StorageKey).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.ReportId, x.CreatedAtUtc });
            entity.HasOne<Report>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
