export async function tryCatch(promise, defaultResult = null) {
  try {
    const result = await promise;
    return [null, result];
  } catch (error) {
    return [error, defaultResult];
  }
}
