const path = require('path');
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
  mode: 'production',
  entry: './src/prose-mirror-bundle.ts',
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  optimization: {
    minimize: true,
    minimizer: [
      new TerserPlugin({
        terserOptions: {
          keep_classnames: true,
          keep_fnames: true,
        },
      }),
    ],
  },  
  output: {
    filename: 'prose-mirror-bundle.js',
    path: path.resolve(__dirname, 'dist'),
    library: {
      name: 'ProseMirrorBundle',
      type: 'var',
    },
  },
};