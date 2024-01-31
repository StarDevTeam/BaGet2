using System;
using System.Threading;
using System.Threading.Tasks;

using BaGet.Core;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BaGet.Web;

public class SymbolController(
    IAuthenticationService authentication,
    ISymbolIndexingService indexer,
    ISymbolStorageService storage,
    IOptionsSnapshot<BaGetOptions> options,
    ILogger<SymbolController> logger) : Controller
{
    private readonly IAuthenticationService _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
    private readonly ISymbolIndexingService _indexer = indexer ?? throw new ArgumentNullException(nameof(indexer));
    private readonly ISymbolStorageService _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    private readonly IOptionsSnapshot<BaGetOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<SymbolController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // See: https://docs.microsoft.com/en-us/nuget/api/package-publish-resource#push-a-package
    public async Task Upload(CancellationToken cancellationToken)
    {
        if (_options.Value.IsReadOnlyMode || !await _authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        try
        {
            await using var uploadStream = await Request.GetUploadStreamOrNullAsync(cancellationToken);
            if (uploadStream == null)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            var result = await _indexer.IndexAsync(uploadStream, cancellationToken);

            switch (result)
            {
                case SymbolIndexingResult.InvalidSymbolPackage:
                    HttpContext.Response.StatusCode = 400;
                    break;

                case SymbolIndexingResult.PackageNotFound:
                    HttpContext.Response.StatusCode = 404;
                    break;

                case SymbolIndexingResult.Success:
                    HttpContext.Response.StatusCode = 201;
                    break;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown during symbol upload");

            HttpContext.Response.StatusCode = 500;
        }
    }

    public async Task<IActionResult> Get(string file, string key)
    {
        var pdbStream = await _storage.GetPortablePdbContentStreamOrNullAsync(file, key);
        if (pdbStream == null)
        {
            return NotFound();
        }

        return File(pdbStream, "application/octet-stream");
    }
}
