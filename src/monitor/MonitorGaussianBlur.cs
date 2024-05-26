namespace dankeyboard.src.monitor
{
    public static class GaussianBlur {
        public static double[,] ApplyGaussianFilter(double[,] input, double deviation, double sigma) {
            
            // calculate kernel size based on deviation
            int kernelSize = (int)Math.Ceiling(deviation * 3) * 2 + 1;

            // create Gaussian kernel matrix
            double[,] kernel = CreateGaussianKernel(kernelSize, sigma);

            // apply filter data
            double[,] output = new double[input.GetLength(0), input.GetLength(1)];
            for (int y = 0; y < input.GetLength(0); y++) {
                for (int x = 0; x < input.GetLength(1); x++) {
                    // apply kernel convolution
                    output[y, x] = ApplyKernel(input, x, y, kernel);
                }
            }

            return output;
        }

        // create Gaussian kernel
        private static double[,] CreateGaussianKernel(int kernelSize, double sigma) {

            double[,] kernel = new double[kernelSize, kernelSize];
            int center = kernelSize / 2;
            double sum = 0;

            for (int y = 0; y < kernelSize; y++) {
                for (int x = 0; x < kernelSize; x++) {
                    double distance = Math.Sqrt(Math.Pow(x - center, 2) + Math.Pow(y - center, 2));
                    kernel[y, x] = Math.Exp(-(distance * distance) / (2 * sigma * sigma));
                    sum += kernel[y, x];
                }
            }

            // normalize kernel
            for (int y = 0; y < kernelSize; y++) {
                for (int x = 0; x < kernelSize; x++) {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }

        // apply kernel convolution at specified coordinates
        private static double ApplyKernel(double[,] image, int x, int y, double[,] kernel) {
            int kernelCenter = kernel.GetLength(0) / 2;
            double sum = 0;

            for (int ky = 0; ky < kernel.GetLength(0); ky++) {
                for (int kx = 0; kx < kernel.GetLength(1); kx++) {
                    // calculate image coordinates within kernel bounds
                    int imageX = x - kernelCenter + kx;
                    int imageY = y - kernelCenter + ky;

                    // Check if image coordinates are within bounds
                    if (imageX >= 0 && imageX < image.GetLength(1) && imageY >= 0 && imageY < image.GetLength(0)) {
                        sum += image[imageY, imageX] * kernel[ky, kx];
                    }
                }
            }

            return sum;
        }
    }

}
