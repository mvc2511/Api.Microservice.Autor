using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using FluentValidation;
using MediatR;

namespace Api.Microservice.Autor.Aplicacion
{
    //esta clase se encarga del transporte de los datos del controlador hacia la logica de mapeo
    public class Nuevo
    {
        public class Ejecuta : IRequest <string>
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime? FechaNacimiento { get; set; }

            public IFormFile Imagen { get; set; }
           
        }
        //clase para validar la clase ejecuta a traves del apifluent validator
        public class EjecutaValidacion : AbstractValidator<Ejecuta>
        {
            public EjecutaValidacion()
            {
                RuleFor(p => p.Nombre).NotEmpty();
                RuleFor(p => p.Apellido).NotEmpty();
            }
        }

        public class Manejador : IRequestHandler<Ejecuta, string>
        {
            public readonly ContextoAutor _context;
            public Manejador(ContextoAutor context)
            {
                _context = context;
            }

            public async Task<string> Handle(Ejecuta request, CancellationToken cancellationToken)
            {
                //se crea la instancia del autor libro ligada al contexto 
                var autorLibro = new AutorLibro
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    FechaNacimiento = request.FechaNacimiento,
                    AutorLibroGuid = Convert.ToString(Guid.NewGuid())
                };
                //agregamos el onjeto del tipo autor-libro
                _context.AutorLibros.Add(autorLibro);
                //insertamos el valor de inserccion 
                var respuesta = await _context.SaveChangesAsync();
                if (respuesta > 0)
                {
                    return autorLibro.AutorLibroGuid;
                }
                throw new Exception("No se pudo insertar el libro");


            }
        }

    }
}
