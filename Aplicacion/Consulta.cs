using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Microservice.Autor.Aplicacion
{
    public class Consulta
    {
        public class ListaAutor : IRequest<List<AutorDto>>
        {

        }

        public class Manejador : IRequestHandler<ListaAutor, List<AutorDto>>
        {
            private readonly ContextoAutor _context;
            private readonly IMapper _mapper;
            private readonly ImageService.ImageService.ImageServiceClient _grpcClient;

            public Manejador(ContextoAutor context, IMapper mapper, ImageService.ImageService.ImageServiceClient grpcClient)
            {
                this._context = context;
                this._mapper = mapper;
                this._grpcClient = grpcClient;
            }

            public async Task<List<AutorDto>> Handle(ListaAutor request, CancellationToken cancellationToken)
            {
                var autores = await _context.AutorLibros.ToListAsync();
                var autoresDto = _mapper.Map<List<AutorLibro>, List<AutorDto>>(autores);

                foreach (var autor in autoresDto)
                {
                    var grpcRequest = new ImageService.ImagenConsultaRequest
                    {
                        AuthorId = autor.AutorLibroGuid
                    };

                    try
                    {
                        var grpcResponse = await _grpcClient.ObtenerImagenAsync(grpcRequest);

                        if (grpcResponse != null && grpcResponse.Image != null)
                        {
                            // Convertir los bytes de la imagen a cadena Base64 y asignarlos al DTO
                            autor.Imagenes = grpcResponse.Image.ToByteArray();
                            autor.Imagen = Convert.ToBase64String(autor.Imagenes);
                        }
                    }
                    catch (RpcException ex)
                    {
                        // Manejar el error de GRPC
                        Console.WriteLine($"Error al obtener imagen para el autor {autor.AutorLibroGuid}: {ex}");
                    }
                }

                return autoresDto;
            }
        }
    }
}
