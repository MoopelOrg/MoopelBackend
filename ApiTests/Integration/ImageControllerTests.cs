using System.Net;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;

namespace ApiTests.Integration;

public class ImageControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public ImageControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Upload

    [Fact]
    public async Task Upload_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Image.Upload, content: null, _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {response.StatusCode} when uploading without authentication.");
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenAuthenticatedWithEmptyFile()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        using MultipartFormDataContent form = new();
        using ByteArrayContent emptyFile = new([]);
        form.Add(emptyFile, ImageTestData.FormFileFieldName, ImageTestData.EmptyFileName);

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Image.Upload, form, _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected {HttpStatusCode.BadRequest} but got {response.StatusCode} when uploading an empty image file.");
    }

    #endregion

    #region Get Image

    [Fact]
    public async Task GetImage_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Image.Generate.ImageById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {response.StatusCode} when retrieving an image without authentication.");
    }

    [Fact]
    public async Task GetImage_ReturnsNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Image.Generate.ImageById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected {HttpStatusCode.NotFound} but got {response.StatusCode} when retrieving a missing image.");
    }

    #endregion

    #region Get Thumbnail

    [Fact]
    public async Task GetThumbnail_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Image.Generate.ThumbnailById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {response.StatusCode} when retrieving an image thumbnail without authentication.");
    }

    [Fact]
    public async Task GetThumbnail_ReturnsNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Image.Generate.ThumbnailById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected {HttpStatusCode.NotFound} but got {response.StatusCode} when retrieving a missing image thumbnail.");
    }

    #endregion
}
