(ns frontend.errors)

(def error-translation-string-map
  {"duplicate username" ::duplicate-username
   "invalid username or password" ::invalid-username-or-password
   "invalid token" ::expired-token
   "habit not found" ::habit-not-found
   "completion type not found" ::completion-type-not-found
   "completion not found" ::completion-not-found
   "unable to delete completion type with existing completions" ::unable-to-delete-completion-type})

(def error-translation-code-map
  {403 ::expired-token
   401 ::expired-token})

(defn response->error-key [r]
  (or (error-translation-string-map (:response r))
      (error-translation-code-map (:status r))
      ::unknown-error))
