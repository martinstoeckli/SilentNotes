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
    minimize: false,
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
    environment: {
      // The environment supports arrow functions ('() => { ... }').
      arrowFunction: false, // default: true
      // The environment supports BigInt as literal (123n).
      bigIntLiteral: false, // default: false
      // The environment supports const and let for variable declarations.
      const: false, // default: true
      // The environment supports destructuring ('{ a, b } = obj').
      destructuring: false, // default: true
      // The environment supports an async import() function to import EcmaScript modules.
      dynamicImport: false, // default: false
      // The environment supports 'for of' iteration ('for (const x of array) { ... }').
      forOf: false, // default: true
      // The environment supports ECMAScript Module syntax to import ECMAScript modules (import ... from '...').
      module: false, // default: false
      // The environment supports optional chaining ('obj?.a' or 'obj?.()').
      optionalChaining: false, // default: true
      // The environment supports template literals.
      templateLiteral: false, // default: true
    },
  },
};