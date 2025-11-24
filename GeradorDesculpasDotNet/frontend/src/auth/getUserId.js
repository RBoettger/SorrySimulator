export function getUserIdFromToken(token) {
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.UserId || payload.userId || payload.sub || null;
  } catch {
    return null;
  }
}