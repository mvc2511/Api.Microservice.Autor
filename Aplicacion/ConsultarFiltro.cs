using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using ImageService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Microservice.Autor.Aplicacion
{
    public class ConsultarFiltro
    {
        public class AutorUnico : IRequest<AutorDto>
        {
            public string AutoGuid { get; set; }
        }

        public class Manejador : IRequestHandler<AutorUnico, AutorDto>
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
            public async Task<AutorDto> Handle(AutorUnico request, CancellationToken cancellationToken)
            {
                var autor = await _context.AutorLibros
                .Where(p => p.AutorLibroGuid == request.AutoGuid).FirstOrDefaultAsync();

                if (autor == null)
                {
                    throw new Exception("No se encontroal autor");

                }
                var autorDto = _mapper.Map<AutorLibro, AutorDto>(autor);

                // Obtener imagen asociada al autor
                var grpcRequest = new ImagenConsultaRequest
                {
                    AuthorId = autor.AutorLibroGuid
                };
                var grpcResponse = await _grpcClient.ObtenerImagenAsync(grpcRequest);

                if (grpcResponse != null && grpcResponse.Image != null)
                {
                    autorDto.Imagenes = grpcResponse.Image.ToByteArray();
                }

                return autorDto;
            }
        }


    }
}
