using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app)
        {

            var rotasEtudantes = app.MapGroup("/estudantes");

            rotasEtudantes.MapPost("", async
                (AddEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {

                var jaExiste = await context.Estudantes.AnyAsync(e => e.Nome == request.Nome, ct);
                var novoEstudante = new Estudante(request.Nome);

                if(jaExiste)
                {
                    return Results.Conflict("Ja existe");
                }

                await context.Estudantes.AddAsync(novoEstudante, ct);
                await context.SaveChangesAsync(ct);

                var estudanteRetorno = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                return Results.Ok(estudanteRetorno);
            });

            rotasEtudantes.MapGet("", async 
                (AppDbContext context, CancellationToken ct) =>
            {
                var estudantes = await context.
                Estudantes
                .Where(estudante => estudante.Ativo)
                .Select(estudante => new EstudanteDto(estudante.Id, estudante.Nome))
                .ToListAsync(ct);

                return Results.Ok(estudantes);
            });

            rotasEtudantes.MapPut("{id:guid}", async
                (Guid id, UpdateEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes
                .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if(estudante is null)
                {
                    return Results.NotFound();
                }

                estudante.AtualizarNome(request.Nome);
                await context.SaveChangesAsync(ct);
                return Results.Ok(new EstudanteDto(estudante.Id, estudante.Nome));
            });

            rotasEtudantes.MapDelete("{id:guid}", async
                               (Guid id, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes
                .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if(estudante is null)
                {
                    return Results.NotFound();
                }

                estudante.Desativar();
                await context.SaveChangesAsync(ct);
                return Results.NoContent();
            });
        }
    }
}
