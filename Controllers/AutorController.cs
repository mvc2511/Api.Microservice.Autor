using Api.Microservice.Autor.Aplicacion;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageService;

namespace Api.Microservice.Autor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutorController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ImageService.ImageService.ImageServiceClient _grpcClient;
        public AutorController(IMediator mediator, ImageService.ImageService.ImageServiceClient grpcClient)
        {
            this._mediator = mediator;
            this._grpcClient = grpcClient;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Crear([FromForm] Nuevo.Ejecuta data)
        {
            var idAutor = await _mediator.Send(data);
    

            if (data.Imagen != null)
            {
                using var ms = new MemoryStream();
                await data.Imagen.CopyToAsync(ms);
                var imagenBytes = ms.ToArray();

                var grpcRequest = new ImageService.InsertImageRequest
                {
                    Image = Google.Protobuf.ByteString.CopyFrom(imagenBytes),
                    AuthorId = idAutor
                };

                var grpcResponse = await _grpcClient.InsertImageAsync(grpcRequest);
                if (!grpcResponse.Message.Equals("Imagen guardada correctamente."))
                {
                    return BadRequest("Error al guardar la imagen.");
                }
            }
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<AutorDto>>> GetAutores()
        {
            return await _mediator.Send(new Consulta.ListaAutor());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AutorDto>> GetAutorLibro(string id)
        {
            return await _mediator.Send(new ConsultarFiltro.AutorUnico { AutoGuid = id });
        }
    }
}
