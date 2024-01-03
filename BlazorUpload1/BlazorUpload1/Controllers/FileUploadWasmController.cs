using BlazorUpload1.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BlazorUpload1.Controllers;

[ApiController]
[Route("[controller]")]
public class FileUploadWasmController : ControllerBase
{
    private readonly IHostEnvironment env;
    private readonly ILogger<FileUploadWasmController> logger;

    public FileUploadWasmController(IHostEnvironment env,
        ILogger<FileUploadWasmController> logger)
    {
        this.env = env;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<IList<UploadResult>>> PostFile(
        [FromForm] IEnumerable<IFormFile> arquivosEnviados)
    {
        var numeroArquivosPermitidos = 3;
        long tamanhoMaximoArquivo = 1024 * 1024;
        var arquivosProcessados = 0;
        var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
        List<UploadResult> resultadoUploads = new();

        foreach (var arquivo in arquivosEnviados)
        {
            var uploadResult = new UploadResult();
            string nomeSeguroArquivo;

            var nomeNaoSeguroArquivo = arquivo.FileName;
            uploadResult.FileName = nomeNaoSeguroArquivo;
            var nomeSeguroArquivoParaExibir = WebUtility.HtmlEncode(nomeNaoSeguroArquivo);

            if (arquivosProcessados < numeroArquivosPermitidos)
            {
                if (arquivo.Length == 0)
                {
                    logger.LogInformation("O tamanho do arquivo {FileName} é 0 (Err: 1)",
                                          nomeSeguroArquivoParaExibir);

                    uploadResult.ErrorCode = 1;
                }
                else if (arquivo.Length > tamanhoMaximoArquivo)
                {
                    logger.LogInformation("O arquivo {FileName} de {Length} bytes é  " +
                                "maior que o limite ({Limit} bytes) permitido (Err: 2)",
                                nomeSeguroArquivoParaExibir, arquivo.Length, tamanhoMaximoArquivo);

                    uploadResult.ErrorCode = 2;
                }
                else
                {
                    try
                    {
                        var extensaoArquivo = Path.GetExtension(arquivo.FileName);
                        nomeSeguroArquivo = Path.GetRandomFileName();
                        nomeSeguroArquivo = nomeSeguroArquivo + extensaoArquivo;
                        var caminhoArquivo = Path.Combine(env.ContentRootPath,"Uploads",nomeSeguroArquivo);

                        await using FileStream fs = new(caminhoArquivo, FileMode.Create);
                        await arquivo.CopyToAsync(fs);

                        logger.LogInformation("{FileName} foi salvo em : {Path}",
                                              nomeSeguroArquivoParaExibir, caminhoArquivo);

                        uploadResult.Uploaded = true;
                        uploadResult.StoredFileName = nomeSeguroArquivo;
                    }
                    catch (IOException ex)
                    {
                        logger.LogError("{FileName} error on upload (Err: 3): {Message}",
                               nomeSeguroArquivoParaExibir, ex.Message);

                        uploadResult.ErrorCode = 3;
                    }
                }

                arquivosProcessados++;
            }
            else
            {
                logger.LogInformation("{FileName} não enviado, pois " +
                    "o request excedeu o número máximo ({Count}) de arquivos permitidos (Err: 4)",
                    nomeSeguroArquivoParaExibir, numeroArquivosPermitidos);

                uploadResult.ErrorCode = 4;
            }

            resultadoUploads.Add(uploadResult);
        }

        return new CreatedResult(resourcePath, resultadoUploads);
    }
}
