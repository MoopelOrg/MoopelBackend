using System.Net;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelObjects;

using static ApiTests.TestHelper;

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
            await client.PostAsync(ApiRoutes.Image.Upload, content: null, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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
            await client.PostAsync(ApiRoutes.Image.Upload, form, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.BadRequest,
            userMessage: await TMsg(HttpStatusCode.BadRequest, response));
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
            await client.GetAsync(ApiRoutes.Image.Generate.ImageById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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
            await client.GetAsync(ApiRoutes.Image.Generate.ImageById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
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
            await client.GetAsync(ApiRoutes.Image.Generate.ThumbnailById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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
            await client.GetAsync(ApiRoutes.Image.Generate.ThumbnailById(ImageTestData.MissingImageId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion
}
