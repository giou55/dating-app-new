import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { map, Observable } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef?: BsModalRef<ConfirmDialogComponent>;

  constructor(private modalService: BsModalService) { }

  confirm(
    title = 'Επιβεβαίωση',
    message = 'Είστε σίγουροι ότι θέλετε να το κάνετε αυτό?',
    btnOkText = 'Ναι',
    btnCancelText = 'Ακύρωση'
  ) : Observable<boolean> {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        title, message, btnOkText, btnCancelText
      }
    };

    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);

    return this.bsModalRef.onHidden!.pipe(
      map(() => {
        return this.bsModalRef!.content!.result;
      })
    )

  }
}
