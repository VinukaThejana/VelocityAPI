using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

using VelocityAPI.Models;

namespace VelocityAPI.Application.S3;

public class S3
{
    public static string GetS3ObjectUrl(IOptions<AppSettings> settings, string objectKey)
    {
        return $"https://{settings.Value.AwsBucket}.s3.{settings.Value.AwsRegion}.amazonaws.com/{objectKey}";
    }

    public static async Task DeleteS3FolderAsync(IAmazonS3 s3Client, string bucketName, string folderPrefix)
    {
        var listRequest = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = folderPrefix,
        };

        ListObjectsV2Response listResponse;

        do
        {
            listResponse = await s3Client.ListObjectsV2Async(listRequest);
            if (listResponse.S3Objects == null || listResponse.S3Objects.Count == 0) return;

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = listResponse.S3Objects.Select(obj => new KeyVersion
                {
                    Key = obj.Key
                }).ToList()
            };

            await s3Client.DeleteObjectsAsync(deleteRequest);

            listRequest.ContinuationToken = listResponse.NextContinuationToken;
        } while (listResponse.IsTruncated ?? false);
    }
}
