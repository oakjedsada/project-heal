using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MindCheck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flow_transitions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    from_instrument_id = table.Column<int>(type: "integer", nullable: true),
                    condition_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: true),
                    condition_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    to_instrument_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flow_transitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "instruments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_instruments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "responses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    choice_id = table.Column<int>(type: "integer", nullable: false),
                    answered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_responses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrument_id = table.Column<int>(type: "integer", nullable: false),
                    total_score = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    next_action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "risk_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    instrument_id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    @operator = table.Column<string>(name: "operator", type: "character varying(30)", maxLength: 30, nullable: false),
                    threshold = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_risk_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scoring_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    instrument_id = table.Column<int>(type: "integer", nullable: false),
                    min_score = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    interpretation = table.Column<string>(type: "text", nullable: false),
                    advice = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoring_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anon_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    current_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    instrument_id = table.Column<int>(type: "integer", nullable: false),
                    order_no = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    question_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_questions_instruments_instrument_id",
                        column: x => x.instrument_id,
                        principalTable: "instruments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "choices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    order_no = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_choices", x => x.id);
                    table.ForeignKey(
                        name: "fk_choices_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "flow_transitions",
                columns: new[] { "id", "condition_type", "condition_value", "from_instrument_id", "question_id", "to_instrument_id" },
                values: new object[,]
                {
                    { 1, "Always", null, null, null, 1 },
                    { 2, "Always", null, 1, null, 2 },
                    { 3, "ScoreLevelEquals", "Positive", 2, null, 3 },
                    { 4, "QuestionScoreAtLeast", "1", 3, 16, 4 }
                });

            migrationBuilder.InsertData(
                table: "instruments",
                columns: new[] { "id", "code", "is_active", "name", "source", "version" },
                values: new object[,]
                {
                    { 1, "ST5", true, "ST-5 (placeholder)", "PLACEHOLDER-DO-NOT-USE-CLINICALLY", "1.0" },
                    { 2, "2Q", true, "2Q (placeholder)", "PLACEHOLDER-DO-NOT-USE-CLINICALLY", "1.0" },
                    { 3, "9Q", true, "9Q (placeholder)", "PLACEHOLDER-DO-NOT-USE-CLINICALLY", "1.0" },
                    { 4, "8Q", true, "8Q (placeholder)", "PLACEHOLDER-DO-NOT-USE-CLINICALLY", "1.0" }
                });

            migrationBuilder.InsertData(
                table: "risk_rules",
                columns: new[] { "id", "action", "instrument_id", "question_id", "operator", "threshold" },
                values: new object[] { 1, "emergency", 4, 24, "GreaterThanOrEqual", 1 });

            migrationBuilder.InsertData(
                table: "scoring_rules",
                columns: new[] { "id", "advice", "instrument_id", "interpretation", "level", "max_score", "min_score" },
                values: new object[,]
                {
                    { 1, "[PLACEHOLDER] คำแนะนำระดับต่ำ", 1, "[PLACEHOLDER] ระดับความเครียดต่ำ", "Low", 4, 0 },
                    { 2, "[PLACEHOLDER] คำแนะนำระดับปานกลาง", 1, "[PLACEHOLDER] ระดับความเครียดปานกลาง", "Medium", 9, 5 },
                    { 3, "[PLACEHOLDER] คำแนะนำระดับสูง", 1, "[PLACEHOLDER] ระดับความเครียดสูง", "High", 15, 10 },
                    { 4, "[PLACEHOLDER] คำแนะนำ", 2, "[PLACEHOLDER] ไม่พบสัญญาณ", "Negative", 0, 0 },
                    { 5, "[PLACEHOLDER] คำแนะนำ", 2, "[PLACEHOLDER] พบสัญญาณ ควรประเมินต่อ", "Positive", 2, 1 },
                    { 6, "[PLACEHOLDER] คำแนะนำ", 3, "[PLACEHOLDER] ระดับต่ำ", "Low", 9, 0 },
                    { 7, "[PLACEHOLDER] คำแนะนำ", 3, "[PLACEHOLDER] ระดับปานกลาง", "Medium", 19, 10 },
                    { 8, "[PLACEHOLDER] คำแนะนำ", 3, "[PLACEHOLDER] ระดับสูง", "High", 27, 20 },
                    { 9, "[PLACEHOLDER] คำแนะนำ", 4, "[PLACEHOLDER] ระดับต่ำ", "Low", 7, 0 },
                    { 10, "[PLACEHOLDER] คำแนะนำ", 4, "[PLACEHOLDER] ระดับปานกลาง", "Medium", 15, 8 },
                    { 11, "[PLACEHOLDER] คำแนะนำ", 4, "[PLACEHOLDER] ระดับสูง", "High", 24, 16 }
                });

            migrationBuilder.InsertData(
                table: "questions",
                columns: new[] { "id", "instrument_id", "order_no", "question_type", "text" },
                values: new object[,]
                {
                    { 1, 1, 1, "SingleChoice", "[PLACEHOLDER] ST-5 ข้อที่ 1" },
                    { 2, 1, 2, "SingleChoice", "[PLACEHOLDER] ST-5 ข้อที่ 2" },
                    { 3, 1, 3, "SingleChoice", "[PLACEHOLDER] ST-5 ข้อที่ 3" },
                    { 4, 1, 4, "SingleChoice", "[PLACEHOLDER] ST-5 ข้อที่ 4" },
                    { 5, 1, 5, "SingleChoice", "[PLACEHOLDER] ST-5 ข้อที่ 5" },
                    { 6, 2, 1, "SingleChoice", "[PLACEHOLDER] 2Q ข้อที่ 1" },
                    { 7, 2, 2, "SingleChoice", "[PLACEHOLDER] 2Q ข้อที่ 2" },
                    { 8, 3, 1, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 1" },
                    { 9, 3, 2, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 2" },
                    { 10, 3, 3, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 3" },
                    { 11, 3, 4, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 4" },
                    { 12, 3, 5, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 5" },
                    { 13, 3, 6, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 6" },
                    { 14, 3, 7, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 7" },
                    { 15, 3, 8, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 8" },
                    { 16, 3, 9, "SingleChoice", "[PLACEHOLDER] 9Q ข้อที่ 9" },
                    { 17, 4, 1, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 1" },
                    { 18, 4, 2, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 2" },
                    { 19, 4, 3, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 3" },
                    { 20, 4, 4, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 4" },
                    { 21, 4, 5, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 5" },
                    { 22, 4, 6, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 6" },
                    { 23, 4, 7, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 7" },
                    { 24, 4, 8, "SingleChoice", "[PLACEHOLDER] 8Q ข้อที่ 8" }
                });

            migrationBuilder.InsertData(
                table: "choices",
                columns: new[] { "id", "label", "order_no", "question_id", "score" },
                values: new object[,]
                {
                    { 11, "ไม่เลย", 1, 1, 0 },
                    { 12, "เล็กน้อย", 2, 1, 1 },
                    { 13, "ปานกลาง", 3, 1, 2 },
                    { 14, "มาก", 4, 1, 3 },
                    { 21, "ไม่เลย", 1, 2, 0 },
                    { 22, "เล็กน้อย", 2, 2, 1 },
                    { 23, "ปานกลาง", 3, 2, 2 },
                    { 24, "มาก", 4, 2, 3 },
                    { 31, "ไม่เลย", 1, 3, 0 },
                    { 32, "เล็กน้อย", 2, 3, 1 },
                    { 33, "ปานกลาง", 3, 3, 2 },
                    { 34, "มาก", 4, 3, 3 },
                    { 41, "ไม่เลย", 1, 4, 0 },
                    { 42, "เล็กน้อย", 2, 4, 1 },
                    { 43, "ปานกลาง", 3, 4, 2 },
                    { 44, "มาก", 4, 4, 3 },
                    { 51, "ไม่เลย", 1, 5, 0 },
                    { 52, "เล็กน้อย", 2, 5, 1 },
                    { 53, "ปานกลาง", 3, 5, 2 },
                    { 54, "มาก", 4, 5, 3 },
                    { 61, "ไม่มี", 1, 6, 0 },
                    { 62, "มี", 2, 6, 1 },
                    { 71, "ไม่มี", 1, 7, 0 },
                    { 72, "มี", 2, 7, 1 },
                    { 81, "ไม่เลย", 1, 8, 0 },
                    { 82, "เล็กน้อย", 2, 8, 1 },
                    { 83, "ปานกลาง", 3, 8, 2 },
                    { 84, "มาก", 4, 8, 3 },
                    { 91, "ไม่เลย", 1, 9, 0 },
                    { 92, "เล็กน้อย", 2, 9, 1 },
                    { 93, "ปานกลาง", 3, 9, 2 },
                    { 94, "มาก", 4, 9, 3 },
                    { 101, "ไม่เลย", 1, 10, 0 },
                    { 102, "เล็กน้อย", 2, 10, 1 },
                    { 103, "ปานกลาง", 3, 10, 2 },
                    { 104, "มาก", 4, 10, 3 },
                    { 111, "ไม่เลย", 1, 11, 0 },
                    { 112, "เล็กน้อย", 2, 11, 1 },
                    { 113, "ปานกลาง", 3, 11, 2 },
                    { 114, "มาก", 4, 11, 3 },
                    { 121, "ไม่เลย", 1, 12, 0 },
                    { 122, "เล็กน้อย", 2, 12, 1 },
                    { 123, "ปานกลาง", 3, 12, 2 },
                    { 124, "มาก", 4, 12, 3 },
                    { 131, "ไม่เลย", 1, 13, 0 },
                    { 132, "เล็กน้อย", 2, 13, 1 },
                    { 133, "ปานกลาง", 3, 13, 2 },
                    { 134, "มาก", 4, 13, 3 },
                    { 141, "ไม่เลย", 1, 14, 0 },
                    { 142, "เล็กน้อย", 2, 14, 1 },
                    { 143, "ปานกลาง", 3, 14, 2 },
                    { 144, "มาก", 4, 14, 3 },
                    { 151, "ไม่เลย", 1, 15, 0 },
                    { 152, "เล็กน้อย", 2, 15, 1 },
                    { 153, "ปานกลาง", 3, 15, 2 },
                    { 154, "มาก", 4, 15, 3 },
                    { 161, "ไม่เลย", 1, 16, 0 },
                    { 162, "เล็กน้อย", 2, 16, 1 },
                    { 163, "ปานกลาง", 3, 16, 2 },
                    { 164, "มาก", 4, 16, 3 },
                    { 171, "ไม่เลย", 1, 17, 0 },
                    { 172, "เล็กน้อย", 2, 17, 1 },
                    { 173, "ปานกลาง", 3, 17, 2 },
                    { 174, "มาก", 4, 17, 3 },
                    { 181, "ไม่เลย", 1, 18, 0 },
                    { 182, "เล็กน้อย", 2, 18, 1 },
                    { 183, "ปานกลาง", 3, 18, 2 },
                    { 184, "มาก", 4, 18, 3 },
                    { 191, "ไม่เลย", 1, 19, 0 },
                    { 192, "เล็กน้อย", 2, 19, 1 },
                    { 193, "ปานกลาง", 3, 19, 2 },
                    { 194, "มาก", 4, 19, 3 },
                    { 201, "ไม่เลย", 1, 20, 0 },
                    { 202, "เล็กน้อย", 2, 20, 1 },
                    { 203, "ปานกลาง", 3, 20, 2 },
                    { 204, "มาก", 4, 20, 3 },
                    { 211, "ไม่เลย", 1, 21, 0 },
                    { 212, "เล็กน้อย", 2, 21, 1 },
                    { 213, "ปานกลาง", 3, 21, 2 },
                    { 214, "มาก", 4, 21, 3 },
                    { 221, "ไม่เลย", 1, 22, 0 },
                    { 222, "เล็กน้อย", 2, 22, 1 },
                    { 223, "ปานกลาง", 3, 22, 2 },
                    { 224, "มาก", 4, 22, 3 },
                    { 231, "ไม่เลย", 1, 23, 0 },
                    { 232, "เล็กน้อย", 2, 23, 1 },
                    { 233, "ปานกลาง", 3, 23, 2 },
                    { 234, "มาก", 4, 23, 3 },
                    { 241, "ไม่เลย", 1, 24, 0 },
                    { 242, "เล็กน้อย", 2, 24, 1 },
                    { 243, "ปานกลาง", 3, 24, 2 },
                    { 244, "มาก", 4, 24, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_choices_question_id",
                table: "choices",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_instruments_code",
                table: "instruments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_questions_instrument_id",
                table: "questions",
                column: "instrument_id");

            migrationBuilder.CreateIndex(
                name: "ix_responses_session_id_question_id",
                table: "responses",
                columns: new[] { "session_id", "question_id" });

            migrationBuilder.CreateIndex(
                name: "ix_results_session_id",
                table: "results",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_anon_token",
                table: "sessions",
                column: "anon_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "choices");

            migrationBuilder.DropTable(
                name: "flow_transitions");

            migrationBuilder.DropTable(
                name: "responses");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "risk_rules");

            migrationBuilder.DropTable(
                name: "scoring_rules");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "instruments");
        }
    }
}
